using Microsoft.EntityFrameworkCore;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
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
				return ServiceResult.Fail(403, "Forbidden");

			var user = await _db.Users.Include(_ => _.Authentication).FirstOrDefaultAsync(_ => _.Email == email);
			if (user == null || !user.IsEmailConfirmed || !(user.Authentication is BasicFlowAuthenticationData basicAuth))
				return ServiceResult.Success();

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

			var resetStringHash = _secureTokensProvider.HashSecureToken(passwordResetString);
			var authData = await _db.AuthenticationData
				.OfType<BasicFlowAuthenticationData>()
				.FirstOrDefaultAsync(_ => _.PasswordResetString == resetStringHash);
			if (authData == null || authData.User == null || !authData.User.IsEmailConfirmed)
				return ServiceResult<string>.Fail(403, "Forbidden");
			if (authData.PasswordResetString == null || authData.PasswordResetStringExpiration == null
				|| DateTime.UtcNow > authData.PasswordResetStringExpiration)
				return ServiceResult<string>.Fail(403, "Forbidden");
			authData.PasswordResetString = null;
			authData.PasswordResetStringExpiration = null;
			await _db.SaveChangesAsync();
			return _jwtPasswordResetTokenProvider.GetToken(authData.User.Id.ToString());
		}
		public async Task<IServiceResult> ChangePassword(string userId, string newPassword)
		{
			var validationResult = _passwordValidator.Validate(newPassword);
			if (validationResult.IsError)
				return ServiceResult.Fail(validationResult.Code, validationResult.Error!);
			var user = await _db.Users.FirstOrDefaultAsync(_ => _.Id == new Guid(userId));
			if (user == null)
				return ServiceResult.Fail(403, "Passwords don't match");
			var authData = user.Authentication;
			if (!(authData is BasicFlowAuthenticationData basicAuth))
			{
				return ServiceResult.Fail(400, "Wrong authentication type for this action.");
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
			long resetExpirationTicks = _config.GetValue<long>("PasswordResetString:ExpirationTicks");
			authData.PasswordResetStringExpiration = DateTime.UtcNow + new TimeSpan(resetExpirationTicks);
			resetString = resetStringRaw;
			return authData;
		}
	}
}
