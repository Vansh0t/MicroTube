using Microsoft.Data.SqlClient;
using MicroTube.Data.Access;
using MicroTube.Data.Access.SQLServer;
using MicroTube.Data.Models;
using MicroTube.Services.Cryptography;
using MicroTube.Services.Email;
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
        private readonly IJwtClaims _jwtClaims;
        private readonly IJwtTokenProvider _jwtTokenProvider;
        private readonly ISecureTokensProvider _secureTokensProvider;
        private readonly IAuthenticationEmailManager _authEmailManager;

        public EmailPasswordAuthenticationProvider(IEmailPasswordAuthenticationDataAccess dataAccess,
                                                   IAppUserDataAccess userDataAccess,
                                                   IUsernameValidator usernameValidator,
                                                   IEmailValidator emailValidator,
                                                   IPasswordValidator passwordValidator,
                                                   IPasswordEncryption passwordEncryption,
                                                   ILogger<EmailPasswordAuthenticationProvider> logger,
                                                   IConfiguration configuration,
                                                   IJwtClaims jwtClaims,
                                                   IJwtTokenProvider jwtTokenProvider,
                                                   ISecureTokensProvider secureTokensProvider,
                                                   IAuthenticationEmailManager authEmailManager)
        {
            _dataAccess = dataAccess;
            _usernameValidator = usernameValidator;
            _emailValidator = emailValidator;
            _passwordValidator = passwordValidator;
            _passwordEncryption = passwordEncryption;
            _logger = logger;
            _userDataAccess = userDataAccess;
            _configuration = configuration;
            _jwtClaims = jwtClaims;
            _jwtTokenProvider = jwtTokenProvider;
            _secureTokensProvider = secureTokensProvider;
            _authEmailManager = authEmailManager;
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

            EmailPasswordAuthentication authData = new EmailPasswordAuthentication { PasswordHash = encryptedPassword};
            string confirmationString;
            try
            {
                authData = ApplyEmailConfirmation(authData, out confirmationString);
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
                await _authEmailManager.SendEmailConfirmation(email, confirmationString);
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
        public async Task<IServiceResult<string>> SignIn(string credential, string password)
        {
            if (string.IsNullOrWhiteSpace(credential) || string.IsNullOrWhiteSpace(password))
            {
                return ServiceResult<string>.Fail(400, "Invalid credential or password");
            }

            EmailPasswordAppUser? user = await _dataAccess.GetWithUserByCredential(credential);
            if(user==null)
            {
                return ServiceResult<string>.Fail(400, "Invalid credential or password");
            }
            bool isPasswordValid = _passwordEncryption.Validate(user.Authentication.PasswordHash, password);
            if(!isPasswordValid)
            {
                return ServiceResult<string>.Fail(400, "Invalid credential or password");
            }
            var jwtTokenResult = _jwtTokenProvider.GetToken(_jwtClaims.GetClaims(user));
            return jwtTokenResult;
        }
        
        public async Task<IServiceResult<string>> ConfirmEmail(string stringRaw)
        {
            if (string.IsNullOrWhiteSpace(stringRaw))
                return ServiceResult<string>.Fail(403, "Forbidden");

            string stringHash;
            try
            {
                stringHash = _secureTokensProvider.HashSecureToken(stringRaw);
            }
            catch(FormatException)
            {
                return ServiceResult<string>.Fail(403, "Forbidden");
            }
            var user = await _dataAccess.GetByEmailConfirmationString(stringHash);
            if(user == null)
            {
                _logger.LogCritical($"User tried to confirm email, but not existed in database");
                return ServiceResult<string>.FailInternal();
            }

            var authData = user.Authentication;
            if(authData == null || authData.EmailConfirmationString == null
                || DateTime.UtcNow > authData.EmailConfirmationStringExpiration
                || !_secureTokensProvider.Validate(authData.EmailConfirmationString, stringRaw))
            {
                return ServiceResult<string>.Fail(403, "Forbidden");
            }
            authData.EmailConfirmationString = null;
            authData.EmailConfirmationStringExpiration = null;
            await _dataAccess.UpdateEmailConfirmation(authData, true);
            user.IsEmailConfirmed = true;
            var jwtTokenResult = _jwtTokenProvider.GetToken(_jwtClaims.GetClaims(user));
            return jwtTokenResult;
        }
        public async Task<IServiceResult> StartEmailChange(int userId, string newEmail, string password)
        {
            var validationResult = _emailValidator.Validate(newEmail);
            if (validationResult.IsError)
                return validationResult;

            var userWithSameEmail = await _userDataAccess.GetByEmail(newEmail);
            if (userWithSameEmail != null)
                return ServiceResult.Fail(400, "Email already in use, try another one.");

            var authUser = await _dataAccess.GetWithUser(userId);
            if (authUser == null || authUser.Authentication == null || !authUser.IsEmailConfirmed 
                || !_passwordEncryption.Validate(authUser.Authentication.PasswordHash, password))
                return ServiceResult.Fail(403, "Forbidden");

            authUser.Authentication = ApplyEmailConfirmation(authUser.Authentication, out string confirmationString);
            authUser.Authentication.PendingEmail = newEmail;
            await _dataAccess.UpdateEmailConfirmation(authUser.Authentication, authUser.IsEmailConfirmed);
            await _authEmailManager.SendEmailChangeStart(newEmail, confirmationString);
            return ServiceResult.Success();
        }
        public async Task<IServiceResult> ConfirmEmailChange(string stringRaw)
        {
            if (string.IsNullOrWhiteSpace(stringRaw))
                return ServiceResult<string>.Fail(403, "Forbidden");

            var stringHash = _secureTokensProvider.HashSecureToken(stringRaw);
            var user = await _dataAccess.GetByEmailConfirmationString(stringHash);
            var authData = user?.Authentication;
            if (user == null || authData == null || authData.PendingEmail == null || authData.EmailConfirmationString == null
                || DateTime.UtcNow > authData.EmailConfirmationStringExpiration
                || !_secureTokensProvider.Validate(authData.EmailConfirmationString, stringRaw))
            {
                return ServiceResult.Fail(403, "Forbidden");
            }
            var userWithSameEmail = await _userDataAccess.GetByEmail(authData.PendingEmail);
            if (userWithSameEmail != null)
            {
                return ServiceResult.Fail(400, "Email already in use, try another one.");
            }
            string newEmail = authData.PendingEmail;
            authData.EmailConfirmationString = null;
            authData.EmailConfirmationStringExpiration = null;
            authData.PendingEmail = null;

            await _dataAccess.UpdateEmailAndConfirmation(authData, newEmail, user.IsEmailConfirmed);
            return ServiceResult.Success();
        }


        private EmailPasswordAuthentication ApplyEmailConfirmation(EmailPasswordAuthentication authData, out string confirmationString)
        {
            string confirmationStringRaw = _secureTokensProvider.GenerateSecureToken();
            string confirmationStringHash = _secureTokensProvider.HashSecureToken(confirmationStringRaw);
            authData.EmailConfirmationString = confirmationStringHash;
            long emailConfirmationExpirationTicks = _configuration.GetValue<long>("EmailConfirmationString:ExpirationTicks");
            authData.EmailConfirmationStringExpiration = DateTime.UtcNow + new TimeSpan(emailConfirmationExpirationTicks);
            confirmationString = confirmationStringRaw;
            return authData;
        }
        private EmailPasswordAuthentication ApplyPasswordReset(EmailPasswordAuthentication authData, out string resetString)
        {
            string resetStringRaw = _secureTokensProvider.GenerateSecureToken();
            string resetStringHash = _secureTokensProvider.HashSecureToken(resetStringRaw);
            authData.PasswordResetString = resetStringHash;
            long resetExpirationTicks = _configuration.GetValue<long>("PasswordResetString:ExpirationTicks");
            authData.PasswordResetStringExpiration = DateTime.UtcNow + new TimeSpan(resetExpirationTicks);
            resetString = resetStringRaw;
            return authData;
        }
    }
}
