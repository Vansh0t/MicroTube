using Microsoft.Data.SqlClient;
using MicroTube.Data.Access;
using MicroTube.Data.Access.SQLServer;
using MicroTube.Data.Models;
using MicroTube.Services.Cryptography;
using MicroTube.Services.Users;
using MicroTube.Services.Validation;
using System.Text;

namespace MicroTube.Services.Authentication.Providers
{
    public class EmailPasswordAuthenticationProvider
    {
        private readonly IEmailPasswordAuthenticationDataAccess _dataAccess;
        private readonly IUsernameValidator _usernameValidator;
        private readonly IEmailValidator _emailValidator;
        private readonly IPasswordValidator _passwordValidator;
        private readonly IPasswordEncryption _passwordEncryption;
        private readonly ILogger<EmailPasswordAuthenticationProvider> _logger;
        private readonly IAppUserDataAccess _userDataAccess;
        private readonly IConfiguration _configuration;


        public EmailPasswordAuthenticationProvider(IEmailPasswordAuthenticationDataAccess dataAccess,
                                                   IAppUserDataAccess userDataAccess,
                                                   IUsernameValidator usernameValidator,
                                                   IEmailValidator emailValidator,
                                                   IPasswordValidator passwordValidator,
                                                   IPasswordEncryption passwordEncryption,
                                                   ILogger<EmailPasswordAuthenticationProvider> logger,
                                                   IConfiguration configuration)
        {
            _dataAccess = dataAccess;
            _usernameValidator = usernameValidator;
            _emailValidator = emailValidator;
            _passwordValidator = passwordValidator;
            _passwordEncryption = passwordEncryption;
            _logger = logger;
            _userDataAccess = userDataAccess;
            _configuration = configuration;
        }

        public async Task<IServiceResult<AppUser>> CreateUser(
            string username, string email, string password)
        {
            StringBuilder errors = new StringBuilder();
            var usernameValidation = _usernameValidator.Validate(username);
            if (usernameValidation.IsError)
                errors.AppendLine(usernameValidation.Error);
            var emailValidation = _emailValidator.Validate(email);
            if (emailValidation.IsError)
                errors.AppendLine(emailValidation.Error);
            var passwordValidation = _passwordValidator.Validate(password);
            if (passwordValidation.IsError)
                errors.Append(passwordValidation.Error);
            if(usernameValidation.IsError || emailValidation.IsError || passwordValidation.IsError)
            {
                return ServiceResult<AppUser>.Fail(400, errors.ToString());
            }

            string encryptedPassword;
            try
            {
                encryptedPassword = _passwordEncryption.Encrypt(password);
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Failed to encrypt a password of length " + password.Length);
                return ServiceResult<AppUser>.FailInternal();
            }

            EmailPasswordAuthentication authData = new EmailPasswordAuthentication(encryptedPassword);
            try
            {
                authData = ApplyEmailConfirmation(authData);
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Failed to apply  email confirmation.");
                return ServiceResult<AppUser>.FailInternal();
            }

            int createdUserId;
            try
            {
                createdUserId = await _dataAccess.CreateUser(username, email, authData);
            }
            catch(SqlException e)
            {
                //unique key constraint violation
                if (e.Number == 2627)
                    return ServiceResult<AppUser>.Fail(400, "Username or Email are already in use.");
                _logger.LogError(e, "Failed to insert new user into database with unhandled SQL exception.");
                return ServiceResult<AppUser>.FailInternal();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to insert new user into database with unhandled exception.");
                return ServiceResult<AppUser>.FailInternal();
            }

            var createdUser = await _userDataAccess.Get(createdUserId);
            if(createdUser == null)
            {
                _logger.LogError("Failed to retrieve newly created user with id {id}.", createdUserId);
                return ServiceResult<AppUser>.FailInternal();
            }
            _logger.LogInformation("New user {username} created with id {id}.", username, createdUserId);
            return new ServiceResult<AppUser>(201, createdUser);
        }
        
        public async Task<IServiceResult> ConfirmEmail(int userId, string stringRaw)
        {
            var authData = await _dataAccess.Get(userId);

            if(authData == null || authData.EmailConfirmationString == null || stringRaw == null
                || DateTime.UtcNow > authData.EmailConfirmationStringExpiration
                || !_passwordEncryption.Validate(authData.EmailConfirmationString, stringRaw))
            {
                return ServiceResult.Fail(403, "Forbidden");
            }
            authData.EmailConfirmationString = null;
            authData.EmailConfirmationStringExpiration = null;
            await _dataAccess.UpdateEmailConfirmation(authData, true);
            return ServiceResult.Success();
        }
        public async Task<IServiceResult> StartEmailChange(int userId, string newEmail)
        {
            var validationResult = _emailValidator.Validate(newEmail);
            if (validationResult.IsError)
                return validationResult;

            var emailOccupied = _userDataAccess.GetByEmail(newEmail) != null;
            if (emailOccupied)
                return ServiceResult.Fail(400, "Email already in use, try another one.");

            var authUser = await _dataAccess.GetWithUser(userId);
            if (authUser == null || authUser.Authentication == null || !authUser.IsEmailConfirmed)
                return ServiceResult.Fail(403, "Forbidden");

            authUser.Authentication = ApplyEmailConfirmation(authUser.Authentication);
            authUser.Authentication.PendingEmail = newEmail;
            await _dataAccess.UpdateEmailConfirmation(authUser.Authentication, authUser.IsEmailConfirmed);
            return ServiceResult.Success();
        }
        public async Task<IServiceResult> ConfirmEmailChange(int userId, string stringRaw)
        {
            var authUser = await _dataAccess.GetWithUser(userId);
            var authData = authUser?.Authentication;
            if(authUser == null || authData == null  || authData.PendingEmail  == null || authData.EmailConfirmationString == null || stringRaw == null
                || DateTime.UtcNow > authData.EmailConfirmationStringExpiration
                || !_passwordEncryption.Validate(authData.EmailConfirmationString, stringRaw))
            {
                return ServiceResult.Fail(403, "Forbidden");
            }
            var emailOccupied = _userDataAccess.GetByEmail(authData.PendingEmail) != null;
            if (emailOccupied)
                return ServiceResult.Fail(400, "Email already in use, try another one.");
            string newEmail = authData.PendingEmail;
            authData.EmailConfirmationString = null;
            authData.EmailConfirmationStringExpiration = null;
            authData.PendingEmail = null;

            await _dataAccess.UpdateEmailAndConfirmation(authData, newEmail, authUser.IsEmailConfirmed);
            return ServiceResult.Success();
        }


        private EmailPasswordAuthentication ApplyEmailConfirmation(EmailPasswordAuthentication authData)
        {
            string confirmationStringRaw = Utils.Cryptography.GetSecureRandomString(_configuration.GetValue<int>("EmailConfirmationString:Length"));
            authData.EmailConfirmationString = _passwordEncryption.Encrypt(confirmationStringRaw);
            long emailConfirmationExpirationTicks = _configuration.GetValue<long>("EmailConfirmationString:ExpirationTicks");
            authData.EmailConfirmationStringExpiration = DateTime.UtcNow + new TimeSpan(emailConfirmationExpirationTicks);
            return authData;
        }
        private EmailPasswordAuthentication ApplyPasswordReset(EmailPasswordAuthentication authData)
        {
            string resetStringRaw = Utils.Cryptography.GetSecureRandomString(_configuration.GetValue<int>("PasswordResetString:Length"));
            authData.PasswordResetString = _passwordEncryption.Encrypt(resetStringRaw);
            long resetExpirationTicks = _configuration.GetValue<long>("PasswordResetString:ExpirationTicks");
            authData.PasswordResetStringExpiration = DateTime.UtcNow + new TimeSpan(resetExpirationTicks);
            return authData;
        }
    }
}
