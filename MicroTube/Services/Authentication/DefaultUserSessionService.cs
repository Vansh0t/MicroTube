using Microsoft.EntityFrameworkCore;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.Cryptography;

namespace MicroTube.Services.Authentication
{
	public class DefaultUserSessionService : IUserSessionService
	{
		private readonly ISecureTokensProvider _tokensProvider;
		private readonly IJwtTokenProvider _accessTokenProvider;
		private readonly IConfiguration _config;
		private readonly ILogger<DefaultUserSessionService> _logger;
		private readonly IJwtClaims _claims;
		private readonly MicroTubeDbContext _db;
		public DefaultUserSessionService(
			ISecureTokensProvider tokensProvider,
			IConfiguration config,
			ILogger<DefaultUserSessionService> logger,
			IJwtTokenProvider accessTokenProvider,
			IJwtClaims claims,
			MicroTubeDbContext db)
		{
			_tokensProvider = tokensProvider;
			_config = config;
			_logger = logger;
			_accessTokenProvider = accessTokenProvider;
			_claims = claims;
			_db = db;
		}

		public async Task<IServiceResult<NewSessionResult>> CreateNewSession(string userId)
		{
			string refreshTokenRaw;
			string refreshToken = GetHashedRefreshToken(out refreshTokenRaw);
			var user = await _db.Users.FirstOrDefaultAsync(_ => _.Id == new Guid(userId));
			if (user == null)
			{
				_logger.LogError($"User {userId} tried to create a new session, but wasn't found in database");
				return ServiceResult<NewSessionResult>.FailInternal();
			}

			DateTime issued = DateTime.UtcNow;
			DateTime expires = GetTokenTime(issued);
			AppUserSession session = new() { UserId = new Guid(userId), Expiration = expires, IssuedAt = issued, IsInvalidated = false, Token = refreshToken };
			_db.Add(session);
			await _db.SaveChangesAsync();
			var accessTokenResult = _accessTokenProvider.BuildJWTAccessToken(_claims, user);
			if(accessTokenResult.IsError)
			{
				_logger.LogError($"Failed to create a new access token for user {userId}, Error: {accessTokenResult.Error}");
				return ServiceResult<NewSessionResult>.FailInternal();
			}
			return ServiceResult<NewSessionResult>.Success(new NewSessionResult(session, refreshTokenRaw, accessTokenResult.GetRequiredObject()));
		}
		public async Task<IServiceResult<NewSessionResult>> RefreshSession(string refreshToken)
		{
			if (string.IsNullOrWhiteSpace(refreshToken))
				return ServiceResult<NewSessionResult>.Fail(400, "Invalid token");
			string tokenHash;
			try
			{
				tokenHash = _tokensProvider.HashSecureToken(refreshToken);
			}
			catch (Exception e)
			{
				_logger.LogWarning(e, "Failed to hash a user provided refresh token.");
				return ServiceResult<NewSessionResult>.Fail(403, "Token is expired or invalid");
			}
			var session = await _db.UserSessions
				.Include(_=>_.UsedTokens.Where(_=>_.Token == tokenHash))
				.Include(_=>_.User)
				.FirstOrDefaultAsync(_ => _.Token == tokenHash);
			if(session == null)
			{
				return ServiceResult<NewSessionResult>.Fail(403, "Token is expired or invalid");
			}
			if (session.UsedTokens.Count() > 0)
			{
				_logger.LogWarning($"Got used refresh token. Invalidating session {session.Id}");
				
				await InvalidateSession(session, $"User session {session.UserId} was invalidated due to the same refresh token used twice.");
				return ServiceResult<NewSessionResult>.Fail(403, "Token is expired or invalid");
				//TO DO: Add user email notification, suggest credentials changing, etc.
			}
			if (session == null || session.IsInvalidated || DateTime.UtcNow > session.Expiration)
				return ServiceResult<NewSessionResult>.Fail(403, "Token is expired or invalid");
			session = ApplySessionRefresh(session, out var newRefreshTokenRaw);
			if (session.User == null)
			{
				_logger.LogError($"User {session.UserId} for session {session.Id} wasn't found.");
				return ServiceResult<NewSessionResult>.FailInternal();
			}
			await _db.SaveChangesAsync();
			var accessTokenResult = _accessTokenProvider.BuildJWTAccessToken(_claims, session.User);
			if (accessTokenResult.IsError)
			{
				_logger.LogError($"Failed to create a new access token for user {session.User.Id}, Error: {accessTokenResult.Error}");
				return ServiceResult<NewSessionResult>.FailInternal();
			}
			return ServiceResult<NewSessionResult>.Success(new NewSessionResult(session, newRefreshTokenRaw, accessTokenResult.GetRequiredObject()));
		}
		public async Task<IServiceResult> InvalidateSession(string sessionId, string reason)
		{
			var session = await _db.UserSessions.FirstOrDefaultAsync(_ => _.Id == new Guid(sessionId));
			if (session == null)
				return ServiceResult.Fail(404, "Session does not exist");
			_logger.LogWarning(reason);
			session.IsInvalidated = true;
			await _db.SaveChangesAsync();
			return ServiceResult.Success();
		}
		private AppUserSession ApplySessionRefresh(AppUserSession session, out string newRefreshTokenRaw)
		{
			string newRefreshToken = GetHashedRefreshToken(out newRefreshTokenRaw);
			session.Token = newRefreshToken;
			session.IssuedAt = DateTime.UtcNow;
			session.Expiration = GetTokenTime(session.IssuedAt);
			return session;
		}
		private async Task InvalidateSession(AppUserSession session, string reason)
		{
			_logger.LogWarning(reason);
			session.IsInvalidated = true;
			await _db.SaveChangesAsync();
		}
		private string GetHashedRefreshToken(out string refreshTokenRaw)
		{
			refreshTokenRaw = _tokensProvider.GenerateSecureToken();
			var tokenHash = _tokensProvider.HashSecureToken(refreshTokenRaw);
			return tokenHash;
		}
		private DateTime GetTokenTime(DateTime issued)
		{
			int expirationMinutes = _config.GetRequiredByKey<int>("UserSession:TokenLifetimeMinutes");
			return issued + TimeSpan.FromMinutes(expirationMinutes);
		}

	}
}
