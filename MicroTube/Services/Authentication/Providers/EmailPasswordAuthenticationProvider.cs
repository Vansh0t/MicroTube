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
        private readonly EmailPasswordAuthenticationDataAccess _dataAccess;
        private readonly IUsernameValidator _usernameValidator;
        private readonly IEmailValidator _emailValidator;
        private readonly IPasswordValidator _passwordValidator;
        private readonly IPasswordEncryption _passwordEncryption;
        private readonly ILogger<EmailPasswordAuthenticationProvider> _logger;
        private readonly IUserManager _userManager;

        public EmailPasswordAuthenticationProvider(EmailPasswordAuthenticationDataAccess dataAccess,
                                                   IUsernameValidator usernameValidator,
                                                   IEmailValidator emailValidator,
                                                   IPasswordValidator passwordValidator,
                                                   IPasswordEncryption passwordEncryption,
                                                   ILogger<EmailPasswordAuthenticationProvider> logger,
                                                   IUserManager userManager)
        {
            _dataAccess = dataAccess;
            _usernameValidator = usernameValidator;
            _emailValidator = emailValidator;
            _passwordValidator = passwordValidator;
            _passwordEncryption = passwordEncryption;
            _logger = logger;
            _userManager = userManager;
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

            EmailPasswordAuthentication authData = new EmailPasswordAuthentication(encryptedPassword)
            {
                //TODO These go into configuration
                EmailConfirmationString = Utils.Cryptography.GetSecureRandomString(64),
                EmailConfirmationStringExpiration = DateTime.UtcNow + TimeSpan.FromDays(1),
            };
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

            var createdUser = await _userManager.GetUser(createdUserId);
            if(createdUser.IsError || createdUser.ResultObject == null)
            {
                _logger.LogError("Failed to retrieve newly created user with id {id}.", createdUserId);
                return ServiceResult<AppUser>.FailInternal();
            }
            _logger.LogInformation("New user {username} created with id {id}.", username, createdUserId);
            return new ServiceResult<AppUser>(201, createdUser.ResultObject);
        }
    }
}
