using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.Cryptography;

namespace MicroTube.Services.Authentication
{
	public class DefaultUserSessionService : IUserSessionService
	{
		private readonly IUserSessionDataAccess _dataAccess;
		private readonly ISecureTokensProvider _tokensProvider;
		private readonly IJwtTokenProvider _accessTokenProvider;
		private readonly IConfiguration _config;
		private readonly ILogger<DefaultUserSessionService> _logger;
		private readonly IAppUserDataAccess _userDataAccess;
		private readonly IJwtClaims _claims;

		public DefaultUserSessionService(
			IUserSessionDataAccess dataAccess,
			ISecureTokensProvider tokensProvider,
			IConfiguration config,
			ILogger<DefaultUserSessionService> logger,
			IAppUserDataAccess userDataAccess,
			IJwtTokenProvider accessTokenProvider,
			IJwtClaims claims)
		{
			_dataAccess = dataAccess;
			_tokensProvider = tokensProvider;
			_config = config;
			_logger = logger;
			_userDataAccess = userDataAccess;
			_accessTokenProvider = accessTokenProvider;
			_claims = claims;
		}

		public async Task<IServiceResult<NewSessionResult>> CreateNewSession(int userId)
		{
			string refreshTokenRaw;
			string refreshToken = GetHashedRefreshToken(out refreshTokenRaw);
			var user = await _userDataAccess.Get(userId);
			if (user == null)
			{
				_logger.LogError($"User {userId} tried to create a new session, but wasn't found in database");
				return ServiceResult<NewSessionResult>.FailInternal();
			}

			DateTime issued = DateTime.UtcNow;
			DateTime expires = GetTokenTime(issued);
			await _dataAccess.CreateSession(userId, refreshToken, issued, expires);
			var createdSession = await _dataAccess.GetSessionByToken(refreshToken);
			if (createdSession == null)
				throw new RequiredObjectNotFoundException("Newly created session wasn't added to the database");
			var accessTokenResult = _accessTokenProvider.BuildJWTAccessToken(_claims, user);
			if(accessTokenResult.IsError)
			{
				_logger.LogError($"Failed to create a new access token for user {userId}, Error: {accessTokenResult.Error}");
				return ServiceResult<NewSessionResult>.FailInternal();
			}

			return ServiceResult<NewSessionResult>.Success(new NewSessionResult(createdSession, refreshTokenRaw, accessTokenResult.GetRequiredObject()));
		}
		public async Task<IServiceResult<NewSessionResult>> RefreshSession(string refreshToken)
		{
			if (string.IsNullOrWhiteSpace(refreshToken))
				return ServiceResult<NewSessionResult>.Fail(400, "Invalid token");

			var tokenHash = _tokensProvider.HashSecureToken(refreshToken);

			var session = await _dataAccess.GetSessionByToken(tokenHash);
			
			if (session == null || session.IsInvalidated || DateTime.UtcNow > session.ExpirationDateTime)
				return ServiceResult<NewSessionResult>.Fail(403, "Token is expired or invalid");
			if (session.PreviousToken == refreshToken)
			{
				await InvalidateSession(session, $"User session {session.UserId} was invalidated due to the same refresh token used twice.");
				return ServiceResult<NewSessionResult>.Fail(403, "Token is expired or invalid");
			}
			var user = await _userDataAccess.Get(session.UserId);
			if (user == null)
			{
				_logger.LogError($"User {session.UserId} tried to update a session {session.Id}, but wasn't found in database");
				return ServiceResult<NewSessionResult>.FailInternal();
			}
			string newRefreshTokenRaw;
			string newRefreshToken = GetHashedRefreshToken(out newRefreshTokenRaw);
			session.PreviousToken = refreshToken;
			session.Token = newRefreshToken;
			session.IssuedDateTime = DateTime.UtcNow;
			session.ExpirationDateTime = GetTokenTime(session.IssuedDateTime);
			await _dataAccess.UpdateSession(session);
			var accessTokenResult = _accessTokenProvider.BuildJWTAccessToken(_claims, user);
			if (accessTokenResult.IsError)
			{
				_logger.LogError($"Failed to create a new access token for user {user.Id}, Error: {accessTokenResult.Error}");
				return ServiceResult<NewSessionResult>.FailInternal();
			}
			return ServiceResult<NewSessionResult>.Success(new NewSessionResult(session, newRefreshTokenRaw, accessTokenResult.GetRequiredObject()));
		}
		public async Task InvalidateSession(AppUserSession session, string reason)
		{
			_logger.LogError(reason);
			session.IsInvalidated = true;
			await _dataAccess.UpdateSession(session);
		}
		private string GetHashedRefreshToken(out string refreshTokenRaw)
		{
			refreshTokenRaw = _tokensProvider.GenerateSecureToken();
			return _tokensProvider.HashSecureToken(refreshTokenRaw);
		}
		private DateTime GetTokenTime(DateTime issued)
		{
			int expirationMinutes = _config.GetRequiredByKey<int>("UserSession:TokenLifetimeMinutes");
			return issued + TimeSpan.FromMinutes(expirationMinutes);
		}

	}
}
