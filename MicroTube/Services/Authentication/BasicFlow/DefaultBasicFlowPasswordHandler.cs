using Microsoft.EntityFrameworkCore;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.Cryptography;
using MicroTube.Services.Email;
using MicroTube.Services.Validation;

namespace MicroTube.Services.Authentication.BasicFlow
{
	public class DefaultBasicFlowPasswordHandler : IBasicFlowPasswordHandler
	{
		private readonly ILogger<DefaultBasicFlowPasswordHandler> _logger;
		private readonly IEmailValidator _emailValidator;
		private readonly MicroTubeDbContext _db;
		private readonly IAuthenticationEmailManager _authEmailManager;
		private readonly ISecureTokensProvider _secureTokensProvider;
		private readonly IJwtPasswordResetTokenProvider _jwtPasswordResetTokenProvider;
		private readonly IPasswordValidator _passwordValidator;
		private readonly IPasswordEncryption _passwordEncryption;
		private readonly IConfiguration _config;

		public DefaultBasicFlowPasswordHandler(
			ILogger<DefaultBasicFlowPasswordHandler> logger,
			IEmailValidator emailValidator,
			MicroTubeDbContext db,
			IAuthenticationEmailManager authEmailManager,
			ISecureTokensProvider secureTokensProvider,
			IJwtPasswordResetTokenProvider jwtPasswordResetTokenProvider,
			IPasswordValidator passwordValidator,
			IPasswordEncryption passwordEncryption,
			IConfiguration config)
		{
			_logger = logger;
			_emailValidator = emailValidator;
			_db = db;
			_authEmailManager = authEmailManager;
			_secureTokensProvider = secureTokensProvider;
			_jwtPasswordResetTokenProvider = jwtPasswordResetTokenProvider;
			_passwordValidator = passwordValidator;
			_passwordEncryption = passwordEncryption;
			_config = config;
		}

		public async Task<IServiceResult> StartPasswordReset(string email)
		{
			var emailValidationResult = _emailValidator.Validate(email);
			if (emailValidationResult.IsError)
				return emailValidationResult;

			var user = await _db.Users.Include(_ => _.Authentication).FirstOrDefaultAsync(_ => _.Email == email);
			if (user == null || !user.IsEmailConfirmed || !(user.Authentication is BasicFlowAuthenticationData basicAuth))
				return ServiceResult.Success(); //return success and do not disclose the error as part of the security standards

			var authData = user.Authentication;
			authData = ApplyPasswordReset(basicAuth, out string passwordResetStringRaw);
			await _db.SaveChangesAsync();
			await _authEmailManager.SendPasswordResetStart(user.Email, passwordResetStringRaw);
			return ServiceResult.Success();
		}
		public async Task<IServiceResult<string>> UsePasswordResetString(string passwordResetString)
		{
			if (string.IsNullOrWhiteSpace(passwordResetString))
				return ServiceResult<string>.Fail(403, "Forbidden");
			string resetStringHash;
			try
			{
				resetStringHash = _secureTokensProvider.HashSecureToken(passwordResetString);
			}
			catch(FormatException)
			{
				return ServiceResult<string>.Fail(403, "Forbidden");
			}
			var authData = await _db.AuthenticationData
				.OfType<BasicFlowAuthenticationData>()
				.Include(_=>_.User)
				.FirstOrDefaultAsync(_ => _.PasswordResetString == resetStringHash);
			if (authData == null || authData.User == null /*|| !authData.User.IsEmailConfirmed*/)
				return ServiceResult<string>.Fail(403, "Forbidden");
			if (authData.PasswordResetString == null || authData.PasswordResetStringExpiration == null
				|| DateTime.UtcNow > authData.PasswordResetStringExpiration)
				return ServiceResult<string>.Fail(403, "Forbidden");
			authData.PasswordResetString = null;
			authData.PasswordResetStringExpiration = null;
			string token;
			try
			{
				token = _jwtPasswordResetTokenProvider.GetToken(authData.User.Id.ToString());
			}
			catch(Exception e)
			{
				_logger.LogCritical($"Failed to generate a password reset access token for user {authData.User.Id}");
				return ServiceResult<string>.FailInternal();
			}
			await _db.SaveChangesAsync();
			return ServiceResult<string>.Success( token);
		}
		public async Task<IServiceResult> ChangePassword(string userId, string newPassword)
		{
			if(string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var guidUserId))
				return ServiceResult.Fail(403, "User does not exist.");
			var validationResult = _passwordValidator.Validate(newPassword);
			if (validationResult.IsError)
				return validationResult;
			var user = await _db.Users.Include(_=>_.Authentication).FirstOrDefaultAsync(_ => _.Id == guidUserId);
			if (user == null)
			{
				_logger.LogError("User tried to change email, but wasn't found.");
				return ServiceResult.Fail(403, "User does not exist.");
			}
			var authData = user.Authentication;
			if (!(authData is BasicFlowAuthenticationData basicAuth))
			{
				return ServiceResult.Fail(403, "Wrong authentication type for this action.");
			}
			basicAuth.PasswordResetString = null;
			basicAuth.PasswordResetStringExpiration = null;
			basicAuth.PasswordHash = _passwordEncryption.Encrypt(newPassword);
			await _db.SaveChangesAsync();
			return ServiceResult.Success();
		}


		public BasicFlowAuthenticationData ApplyPasswordReset(BasicFlowAuthenticationData authData, out string resetString)
		{
			string resetStringRaw = _secureTokensProvider.GenerateSecureToken();
			string resetStringHash = _secureTokensProvider.HashSecureToken(resetStringRaw);
			authData.PasswordResetString = resetStringHash;
			var options = _config.GetRequiredByKey<PasswordConfirmationOptions>(PasswordConfirmationOptions.KEY);
			authData.PasswordResetStringExpiration = DateTime.UtcNow + TimeSpan.FromSeconds(options.ExpirationSeconds);
			resetString = resetStringRaw;
			return authData;
		}
	}
}
