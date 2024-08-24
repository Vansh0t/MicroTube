using EntityFramework.Exceptions.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.Cryptography;
using MicroTube.Services.Email;
using MicroTube.Services.Validation;
using System.Text;

namespace MicroTube.Services.Authentication.BasicFlow
{
	public class DefaultBasicFlowAuthenticationProvider : IBasicFlowAuthenticationProvider
	{
		private readonly IUsernameValidator _usernameValidator;
		private readonly IEmailValidator _emailValidator;
		private readonly IPasswordValidator _passwordValidator;
		private readonly IPasswordEncryption _passwordEncryption;
		private readonly ILogger<DefaultBasicFlowAuthenticationProvider> _logger;
		private readonly MicroTubeDbContext _db;
		private readonly IBasicFlowEmailHandler _basicFlowEmailHandler;
		private readonly IAuthenticationEmailManager _emailManager;

		public DefaultBasicFlowAuthenticationProvider(IUsernameValidator usernameValidator,
												   IEmailValidator emailValidator,
												   IPasswordValidator passwordValidator,
												   IPasswordEncryption passwordEncryption,
												   ILogger<DefaultBasicFlowAuthenticationProvider> logger,
												   MicroTubeDbContext db,
												   IBasicFlowEmailHandler basicFlowEmailHandler,
												   IAuthenticationEmailManager emailManager)
		{
			_usernameValidator = usernameValidator;
			_emailValidator = emailValidator;
			_passwordValidator = passwordValidator;
			_passwordEncryption = passwordEncryption;
			_logger = logger;
			_db = db;
			_basicFlowEmailHandler = basicFlowEmailHandler;
			_emailManager = emailManager;
		}

		public async Task<IServiceResult<AppUser>> CreateUser(
			string username, string email, string password)
		{
			var validationResult = ValidateUserCreationParameters(username, email, password);
			if (validationResult.IsError)
			{
				return ServiceResult<AppUser>.Fail(400, validationResult.Error!);
			}

			string encryptedPassword;
			try
			{
				encryptedPassword = _passwordEncryption.Encrypt(password);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to encrypt a password of length " + password.Length);
				return ServiceResult<AppUser>.FailInternal();
			}

			BasicFlowAuthenticationData authData = new BasicFlowAuthenticationData { PasswordHash = encryptedPassword };
			string confirmationString;
			try
			{
				authData = _basicFlowEmailHandler.ApplyEmailConfirmation(authData, out confirmationString);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to apply  email confirmation.");
				return ServiceResult<AppUser>.FailInternal();
			}
			var newUser = new AppUser
			{
				Email = email,
				Username = username,
				Authentication = authData,
				IsEmailConfirmed = false,
				PublicUsername = username
			};
			authData.User = newUser;
			try
			{
				_db.AddRange(newUser, authData);
				await _db.SaveChangesAsync();
				await _emailManager.SendEmailConfirmation(newUser.Email, confirmationString);
			}
			catch (UniqueConstraintException uniqueConstraintException)
			{
				if (uniqueConstraintException.ConstraintProperties == null //Always null for SQLite
					|| uniqueConstraintException.ConstraintProperties.Contains(nameof(AppUser.Username)) 
					|| uniqueConstraintException.ConstraintProperties.Contains(nameof(AppUser.Email)))
				{
					return ServiceResult<AppUser>.Fail(400, "Username or Email are already in use.");
				}
				_logger.LogError(uniqueConstraintException, "Failed to insert new user into database with unhandled SQL exception.");
				return ServiceResult<AppUser>.FailInternal();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to insert new user into database with unhandled exception.");
				return ServiceResult<AppUser>.FailInternal();
			}
			_logger.LogInformation("New user {username} created with id {id}.", newUser.Username, newUser.Id);
			return new ServiceResult<AppUser>(201, newUser);
		}
		public async Task<IServiceResult<AppUser>> SignIn(string credential, string password)
		{
			if (string.IsNullOrWhiteSpace(credential) || string.IsNullOrWhiteSpace(password))
			{
				return ServiceResult<AppUser>.Fail(400, "Invalid credential or password");
			}

			AppUser? user = await _db.Users
				.Include(_ => _.Authentication)
				.AsNoTracking()
				.FirstOrDefaultAsync(_ => _.Email == credential || _.Username == credential);
			if (user == null)
			{
				return ServiceResult<AppUser>.Fail(401, "Invalid credential or password");
			}
			if (!(user.Authentication is BasicFlowAuthenticationData emailPasswordAuthentication))
			{
				return ServiceResult<AppUser>.Fail(401, "Use other authentication type");
			}
			bool isPasswordValid = _passwordEncryption.Validate(emailPasswordAuthentication.PasswordHash, password);
			if (!isPasswordValid)
			{
				return ServiceResult<AppUser>.Fail(401, "Invalid credential or password");
			}
			return ServiceResult<AppUser>.Success(user);
		}
		private IServiceResult ValidateUserCreationParameters(string username, string email, string password)
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
			if (usernameValidation.IsError || emailValidation.IsError || passwordValidation.IsError)
			{
				return ServiceResult.Fail(400, errors.ToString());
			}
			return ServiceResult.Success();
		}

	}
}
