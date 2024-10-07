using Microsoft.EntityFrameworkCore;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.Cryptography;
using MicroTube.Services.Email;
using MicroTube.Services.Validation;

namespace MicroTube.Services.Authentication.BasicFlow
{
	public class DefaultBasicFlowEmailHandler : IBasicFlowEmailHandler
	{
		private readonly ILogger<DefaultBasicFlowEmailHandler> _logger;
		private readonly MicroTubeDbContext _db;
		private readonly ISecureTokensProvider _secureTokensProvider;
		private readonly IConfiguration _config;
		private readonly IAuthenticationEmailManager _authEmailManager;
		private readonly IEmailValidator _emailValidator;
		private readonly IPasswordEncryption _passwordEncryption;

		public DefaultBasicFlowEmailHandler(
			ILogger<DefaultBasicFlowEmailHandler> logger,
			MicroTubeDbContext db,
			ISecureTokensProvider secureTokensProvider,
			IConfiguration config,
			IAuthenticationEmailManager authEmailManager,
			IEmailValidator emailValidator,
			IPasswordEncryption passwordEncryption)
		{
			_logger = logger;
			_db = db;
			_secureTokensProvider = secureTokensProvider;
			_config = config;
			_authEmailManager = authEmailManager;
			_emailValidator = emailValidator;
			_passwordEncryption = passwordEncryption;
		}

		public async Task<IServiceResult> ResendEmailConfirmation(string userId)
		{
			AppUser? authUser = await _db.Users
			.Include(_ => _.Authentication)
			.FirstOrDefaultAsync(_ => _.Id == new Guid(userId));
			if (authUser == null)
			{
				_logger.LogError($"Unable to resend email confirmation: User {userId} does not exist. ");
				return ServiceResult.Fail(404, "User does not exists");
			}
			if (authUser.IsEmailConfirmed)
			{
				return ServiceResult.Fail(400, "Action not required");
			}
			if (authUser.Authentication == null || !(authUser.Authentication is BasicFlowAuthenticationData basicAuth))
			{
				_logger.LogError($"User {userId} does not have a basic auth flow, but attempted to resend email confirmation. Flow: {authUser.Authentication}");
				return ServiceResult.Fail(400, "Invalid authentication type.");
			}
			authUser.Authentication = ApplyEmailConfirmation(basicAuth, out var confirmationString);
			await _db.SaveChangesAsync();
			await _authEmailManager.SendEmailConfirmation(authUser.Email, confirmationString);
			return ServiceResult.Success();
		}
		public async Task<IServiceResult<AppUser>> ConfirmEmail(string stringRaw)
		{
			if (string.IsNullOrWhiteSpace(stringRaw))
				return ServiceResult<AppUser>.Fail(403, "Forbidden");

			string stringHash;
			try
			{
				stringHash = _secureTokensProvider.HashSecureToken(stringRaw);
			}
			catch (FormatException)//TO DO: Use custom exception or ServiceResult?
			{
				return ServiceResult<AppUser>.Fail(403, "Forbidden");
			}
			var authData = await _db.AuthenticationData
				.Include(_ => _.User)
				.OfType<BasicFlowAuthenticationData>()
				.FirstOrDefaultAsync(_ => _.EmailConfirmationString == stringHash);
			if (authData == null || authData.User == null)
			{
				return ServiceResult<AppUser>.Fail(403, "Forbidden");
			}
			if (authData.User.IsEmailConfirmed)
				return await ConfirmEmailChange(stringRaw);

			if (DateTime.UtcNow > authData.EmailConfirmationStringExpiration)
			{
				return ServiceResult<AppUser>.Fail(403, "Forbidden");
			}
			authData.EmailConfirmationString = null;
			authData.EmailConfirmationStringExpiration = null;
			authData.User.IsEmailConfirmed = true;
			await _db.SaveChangesAsync();
			return ServiceResult<AppUser>.Success(authData.User);
		}
		public BasicFlowAuthenticationData ApplyEmailConfirmation(BasicFlowAuthenticationData authData, out string confirmationString)
		{
			string confirmationStringRaw = _secureTokensProvider.GenerateSecureToken();
			string confirmationStringHash = _secureTokensProvider.HashSecureToken(confirmationStringRaw);
			authData.EmailConfirmationString = confirmationStringHash;
			var options = _config.GetRequiredByKey<EmailConfirmationOptions>(EmailConfirmationOptions.KEY);
			authData.EmailConfirmationStringExpiration = DateTime.UtcNow + TimeSpan.FromSeconds(options.ExpirationSeconds);
			confirmationString = confirmationStringRaw;
			return authData;
		}
		public async Task<IServiceResult<AppUser>> ConfirmEmailChange(string stringRaw)
		{
			if (string.IsNullOrWhiteSpace(stringRaw))
				return ServiceResult<AppUser>.Fail(403, "Forbidden");
			string stringHash;
			try
			{
				stringHash = _secureTokensProvider.HashSecureToken(stringRaw);
			}
			catch (FormatException)//TO DO: Use custom exception or ServiceResult?
			{
				return ServiceResult<AppUser>.Fail(403, "Forbidden");
			}
			var authData = await _db.AuthenticationData
				.Include(_ => _.User)
				.OfType<BasicFlowAuthenticationData>()
				.FirstOrDefaultAsync(_ => _.EmailConfirmationString == stringHash);
			if (authData == null || authData.User == null)
			{
				return ServiceResult<AppUser>.Fail(403, "Forbidden");
			}
			var validationResult = await ValidateEmailChange(authData, stringHash);
			if (validationResult.IsError)
				return ServiceResult<AppUser>.Fail(validationResult.Code, validationResult.Error!);
			string newEmail = validationResult.GetRequiredObject();
			authData.EmailConfirmationString = null;
			authData.EmailConfirmationStringExpiration = null;
			authData.PendingEmail = null;
			authData.User.Email = newEmail;
			await _db.SaveChangesAsync();
			return ServiceResult<AppUser>.Success(authData.User);
		}
		public async Task<IServiceResult> StartEmailChange(string userId, string newEmail, string password)
		{
			if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var guidUserId))
				return ServiceResult<AppUser>.Fail(403, "Forbidden");
			var validationResult = _emailValidator.Validate(newEmail);
			if (validationResult.IsError)
				return validationResult;

			var userWithSameEmail = await _db.Users.FirstOrDefaultAsync(_ => _.Email == newEmail);
			if (userWithSameEmail != null)
				return ServiceResult.Fail(400, "Email already in use, try another one.");
			var authUser = await _db.Users.Include(_=>_.Authentication).FirstOrDefaultAsync(_ => _.Id == guidUserId);
			if(authUser == null)
			{
				_logger.LogError($"Failed to start email change process: user with id {userId} or their {nameof(authUser.Authentication)} not found.");
				return ServiceResult.Fail(403, "Forbidden");
			}
			if(!authUser.IsEmailConfirmed)
			{
				return ServiceResult.Fail(403, "Confirm email and try again.");
			}
			if (authUser.Authentication == null || !(authUser.Authentication is BasicFlowAuthenticationData basicAuth))
				return ServiceResult.Fail(403, "Wrong authentication type.");
			if (!_passwordEncryption.Validate(basicAuth.PasswordHash, password))
				return ServiceResult.Fail(403, "Wrong password. Please, try again.");

			authUser.Authentication = ApplyEmailConfirmation(basicAuth, out string confirmationString);
			basicAuth.PendingEmail = newEmail;
			await _db.SaveChangesAsync();
			await _authEmailManager.SendEmailConfirmation(newEmail, confirmationString);
			return ServiceResult.Success();
		}
		private async Task<IServiceResult<string>> ValidateEmailChange(BasicFlowAuthenticationData authData, string confirmationStringHash)
		{
			if (authData.PendingEmail == null 
				|| authData.EmailConfirmationString == null 
				|| authData.EmailConfirmationStringExpiration == null
				|| authData.EmailConfirmationString != confirmationStringHash
				|| DateTime.UtcNow > authData.EmailConfirmationStringExpiration)
			{
				return ServiceResult<string>.Fail(403, "Forbidden");
			}
			var userWithSameEmail = await _db.Users.FirstOrDefaultAsync(_ => _.Email == authData.PendingEmail);
			if (userWithSameEmail != null)
			{
				return ServiceResult<string>.Fail(400, "Email already in use, try another one.");
			}
			return ServiceResult<string>.Success(authData.PendingEmail);
		}
	}
}
