using Microsoft.EntityFrameworkCore;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.ConfigOptions;
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
			if(!Guid.TryParse(userId, out var guidUserId))
			{
				return ServiceResult<NewSessionResult>.Fail(403, "Forbidden");
			}
			var user = await _db.Users.FirstOrDefaultAsync(_ => _.Id == guidUserId);
			if (user == null)
			{
				_logger.LogError($"User {userId} tried to create a new session, but wasn't found in database");
				return ServiceResult<NewSessionResult>.Fail(403, "Forbidden");
			}
			string refreshToken = GetHashedRefreshToken(out var refreshTokenRaw);
			DateTime issued = DateTime.UtcNow;
			DateTime expires = GetTokenTime(issued);
			AppUserSession session = new() { UserId = new Guid(userId), Expiration = expires, IssuedAt = issued, IsInvalidated = false, Token = refreshToken };
			var accessTokenResult = _accessTokenProvider.BuildJWTAccessToken(_claims, user);
			if(accessTokenResult.IsError)
			{
				_logger.LogError($"Failed to create a new access token for user {userId}, Error: {accessTokenResult.Error}");
				return ServiceResult<NewSessionResult>.FailInternal();
			}
			_db.Add(session);
			await _db.SaveChangesAsync();
			return ServiceResult<NewSessionResult>.Success(new NewSessionResult(session, refreshTokenRaw, accessTokenResult.GetRequiredObject()));
		}
		public async Task<IServiceResult<NewSessionResult>> RefreshSession(string refreshToken)
		{
			if (string.IsNullOrWhiteSpace(refreshToken))
				return ServiceResult<NewSessionResult>.Fail(403, "Token is expired or invalid");
			string tokenHash;
			try
			{
				tokenHash = _tokensProvider.HashSecureToken(refreshToken);
			}
			catch (FormatException)
			{
				return ServiceResult<NewSessionResult>.Fail(403, "Token is expired or invalid");
			}
			var session = await _db.UserSessions
				//.Include(_=>_.UsedTokens.Where(_=>_.Token == tokenHash))
				.Include(_=>_.User)
				.FirstOrDefaultAsync(_ => _.Token == tokenHash);
			if(session == null)
			{
				await HandlePotentialReusedToken(tokenHash);
				return ServiceResult<NewSessionResult>.Fail(403, "Token is expired or invalid");
			}
			if (session.IsInvalidated || DateTime.UtcNow > session.Expiration)
			{
				return ServiceResult<NewSessionResult>.Fail(403, "Token is expired or invalid");
			}
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
			if (!Guid.TryParse(sessionId, out var guidSessionId))
			{
				return ServiceResult<NewSessionResult>.Fail(400, "Invalid session id");
			}
			var session = await _db.UserSessions.FirstOrDefaultAsync(_ => _.Id == guidSessionId);
			if (session == null)
				return ServiceResult.Fail(404, "Session does not exist");
			_logger.LogWarning(reason);
			session.IsInvalidated = true;
			await _db.SaveChangesAsync();
			_logger.LogWarning($"Session {sessionId} is invalidated. Reason: {reason}.");
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
		private string GetHashedRefreshToken(out string refreshTokenRaw)
		{
			refreshTokenRaw = _tokensProvider.GenerateSecureToken();
			var tokenHash = _tokensProvider.HashSecureToken(refreshTokenRaw);
			return tokenHash;
		}
		private DateTime GetTokenTime(DateTime issued)
		{
			var options = _config.GetRequiredByKey<UserSessionOptions>(UserSessionOptions.KEY);
			return issued + TimeSpan.FromMinutes(options.TokenLifetimeMinutes);
		}
		private async Task HandlePotentialReusedToken(string tokenHash)
		{
			UsedRefreshToken? usedToken = await _db.UsedRefreshTokens.FirstOrDefaultAsync(_ => _.Token == tokenHash);
			if(usedToken != null)
			{
				var result = await InvalidateSession(usedToken.SessionId.ToString(), $"Got used token {usedToken.Id}");
				if (result.IsError)
					_logger.LogError($"Failed to invalidate session {usedToken.SessionId} due to an error: {result.Error}");
			}
		}
	}
}
