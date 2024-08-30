using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services;
using MicroTube.Services.Authentication;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.Cryptography;
using MicroTube.Tests.Utils;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace MicroTube.Tests.Unit.Authentication
{
	public class DefaultUserSessionServiceTests
	{
		[Fact]
		public async Task CreateNewSession_Success()
		{
			string validRefreshToken = "valid_refresh_token";
			string validRefreshTokenHash = "valid_refresh_token_hash";
			string validJwtAccessToken = "valid_access_token";
			MicroTubeDbContext db = Database.CreateSqliteInMemory();
			AppUser user = new AppUser
			{
				Email = "email@email.com",
				IsEmailConfirmed = false,
				PublicUsername = "username",
				Username = "username",
				Authentication = new BasicFlowAuthenticationData { PasswordHash = "password_hash" }
			};
			user.Authentication.User = user;
			db.Add(user);
			db.SaveChanges();
			IUserSessionService sessionService = CreateSessionService(db, validRefreshToken, validRefreshTokenHash, validJwtAccessToken, "prev_token", "prev_token_hash");
			var sessionResult = await sessionService.CreateNewSession(user.Id.ToString());
			Assert.False(sessionResult.IsError);
			NewSessionResult session = sessionResult.GetRequiredObject();
			Assert.Equal(validJwtAccessToken, session.AccessToken);
			Assert.Equal(validRefreshToken, session.RefreshTokenRaw);
			Assert.True(DateTime.UtcNow - session.Session.IssuedAt < TimeSpan.FromMinutes(1));
			Assert.True(session.Session.Expiration - DateTime.UtcNow < TimeSpan.FromMinutes(1));
			Assert.True(session.Session.Expiration - DateTime.UtcNow > TimeSpan.FromSeconds(30));
			Assert.Equal(user.Id, session.Session.UserId);
			Assert.Equal(validRefreshTokenHash, session.Session.Token);
			Assert.False(session.Session.IsInvalidated);
			var sessionFromDb = db.UserSessions.First(_ => _.Id == session.Session.Id);
			Assert.True(sessionFromDb.IsEqualByContentValues(session.Session));
		}
		[Theory]
		[InlineData(null)]
		[InlineData(" ")]
		[InlineData("")]
		[InlineData("not_guid")]
		[InlineData("47b6697a-1111-1111-990b-1caf155cb708")]
		public async Task CreateNewSession_InvalidUserIdFail(string? userId)
		{
			string validRefreshToken = "valid_refresh_token";
			string validRefreshTokenHash = "valid_refresh_token_hash";
			string validJwtAccessToken = "valid_access_token";
			MicroTubeDbContext db = Database.CreateSqliteInMemory();
			AppUser user = new AppUser
			{
				Id = new Guid("47b6697a-b56b-49a4-990b-1caf155cb708"),
				Email = "email@email.com",
				IsEmailConfirmed = false,
				PublicUsername = "username",
				Username = "username",
				Authentication = new BasicFlowAuthenticationData { PasswordHash = "password_hash" }
			};
			user.Authentication.User = user;
			db.Add(user);
			db.SaveChanges();
			IUserSessionService sessionService = CreateSessionService(db, validRefreshToken, validRefreshTokenHash, validJwtAccessToken, "prev_token", "prev_token_hash");
			var sessionResult = await sessionService.CreateNewSession(userId);
			Assert.True(sessionResult.IsError);
			Assert.Equal(403, sessionResult.Code);
			var sessionFromDb = db.UserSessions.FirstOrDefault(_ => _.UserId == user.Id);
			Assert.Null(sessionFromDb);
		}
		[Fact]
		public async Task RefreshSession_Success()
		{
			string previousRefreshToken = "previous_refresh_token";
			string previousRefreshTokenHash = "previous_refresh_token_hash";
			string validRefreshToken = "valid_refresh_token";
			string validRefreshTokenHash = "valid_refresh_token_hash";
			string validJwtAccessToken = "valid_access_token";
			MicroTubeDbContext db = Database.CreateSqliteInMemory();
			AppUser user = new AppUser
			{
				Email = "email@email.com",
				IsEmailConfirmed = false,
				PublicUsername = "username",
				Username = "username",
				Authentication = new BasicFlowAuthenticationData { PasswordHash = "password_hash" }
			};
			AppUserSession session = new()
			{
				Token = previousRefreshTokenHash,
				Expiration = DateTime.UtcNow + TimeSpan.FromSeconds(60),
				IsInvalidated = false,
				IssuedAt = DateTime.UtcNow,
				User = user
			};
			session.UsedTokens.Add(new UsedRefreshToken { Token = "some_used_token", Session = session });
			user.Authentication.User = user;
			db.AddRange(user, session);
			db.SaveChanges();
			IUserSessionService sessionService = CreateSessionService(db, validRefreshToken, validRefreshTokenHash, validJwtAccessToken, previousRefreshToken, previousRefreshTokenHash);
			var sessionResult = await sessionService.RefreshSession(previousRefreshToken);
			Assert.False(sessionResult.IsError);
			NewSessionResult updatedSession = sessionResult.GetRequiredObject();
			Assert.Equal(validJwtAccessToken, updatedSession.AccessToken);
			Assert.Equal(validRefreshToken, updatedSession.RefreshTokenRaw);
			Assert.True(DateTime.UtcNow - updatedSession.Session.IssuedAt < TimeSpan.FromMinutes(1));
			Assert.True(updatedSession.Session.Expiration - DateTime.UtcNow < TimeSpan.FromMinutes(1));
			Assert.True(updatedSession.Session.Expiration - DateTime.UtcNow > TimeSpan.FromSeconds(30));
			Assert.Equal(user.Id, updatedSession.Session.UserId);
			Assert.Equal(validRefreshTokenHash, updatedSession.Session.Token);
			Assert.False(updatedSession.Session.IsInvalidated);
			var sessionFromDb = db.UserSessions.First(_ => _.Id == updatedSession.Session.Id);
			Assert.True(sessionFromDb.IsEqualByContentValues(updatedSession.Session));
		}
		[Theory]
		[InlineData(null, 30, false)]
		[InlineData("", 30, false)]
		[InlineData(" ", 30, false)]
		[InlineData("invalid_token", 30, false)]
		[InlineData("previous_refresh_token", 61, false)]
		[InlineData("previous_refresh_token", 30, true)]
		public async Task RefreshSession_InvalidOrExpiredRefreshTokenFail(string? refreshToken, int expirationOffsetSeconds, bool isSessionInvalidated)
		{
			string previousRefreshToken = "previous_refresh_token";
			string previousRefreshTokenHash = "previous_refresh_token_hash";
			string validRefreshToken = "valid_refresh_token";
			string validRefreshTokenHash = "valid_refresh_token_hash";
			string validJwtAccessToken = "valid_access_token";
			MicroTubeDbContext db = Database.CreateSqliteInMemory();
			AppUser user = new AppUser
			{
				Email = "email@email.com",
				IsEmailConfirmed = false,
				PublicUsername = "username",
				Username = "username",
				Authentication = new BasicFlowAuthenticationData { PasswordHash = "password_hash" }
			};
			AppUserSession session = new()
			{
				Token = previousRefreshTokenHash,
				Expiration = DateTime.UtcNow + TimeSpan.FromSeconds(60) - TimeSpan.FromSeconds(expirationOffsetSeconds),
				IsInvalidated = isSessionInvalidated,
				IssuedAt = DateTime.UtcNow,
				User = user
			};
			session.UsedTokens.Add(new UsedRefreshToken { Token = "some_used_token", Session = session });
			user.Authentication.User = user;
			db.AddRange(user, session);
			db.SaveChanges();
			IUserSessionService sessionService = CreateSessionService(db, validRefreshToken, validRefreshTokenHash, validJwtAccessToken, previousRefreshToken, previousRefreshTokenHash);
			var sessionResult = await sessionService.RefreshSession(refreshToken);
			Assert.True(sessionResult.IsError);
			Assert.Null(sessionResult.ResultObject);
			Assert.Equal(403, sessionResult.Code);
			var sessionFromDb = db.UserSessions.First(_ => _.Id == session.Id);
			Assert.True(sessionFromDb.IsEqualByContentValues(session));
		}
		[Fact]
		public async Task RefreshSession_SessionInvalidationFail()
		{
			string previousRefreshToken = "previous_refresh_token";
			string previousRefreshTokenHash = "previous_refresh_token_hash";
			string validRefreshToken = "valid_refresh_token";
			string validRefreshTokenHash = "valid_refresh_token_hash";
			string validJwtAccessToken = "valid_access_token";
			MicroTubeDbContext db = Database.CreateSqliteInMemory();
			AppUser user = new AppUser
			{
				Email = "email@email.com",
				IsEmailConfirmed = false,
				PublicUsername = "username",
				Username = "username",
				Authentication = new BasicFlowAuthenticationData { PasswordHash = "password_hash" }
			};
			AppUserSession session = new()
			{
				Token = "current_token",
				Expiration = DateTime.UtcNow + TimeSpan.FromSeconds(60),
				IsInvalidated = false,
				IssuedAt = DateTime.UtcNow,
				User = user
			};
			session.UsedTokens.Add(new UsedRefreshToken { Token = previousRefreshTokenHash, Session = session });
			user.Authentication.User = user;
			db.AddRange(user, session);
			db.SaveChanges();
			IUserSessionService sessionService = CreateSessionService(db, validRefreshToken, validRefreshTokenHash, validJwtAccessToken, previousRefreshToken, previousRefreshTokenHash);
			var sessionResult = await sessionService.RefreshSession(previousRefreshToken);
			Assert.True(sessionResult.IsError);
			Assert.Null(sessionResult.ResultObject);
			Assert.Equal(403, sessionResult.Code);
			var sessionFromDb = db.UserSessions.First(_ => _.Id == session.Id);
			Assert.True(sessionFromDb.IsInvalidated);
		}
		public IUserSessionService CreateSessionService(
			MicroTubeDbContext db,
			string validRefreshToken,
			string validRefreshTokenHash,
			string validJwtAccessToken,
			string validPreviousRefreshToken,
			string validPreviousRefreshTokenHash)
		{
			ISecureTokensProvider secureTokensProvider = Substitute.For<ISecureTokensProvider>();
			secureTokensProvider.GenerateSecureToken().Returns(validRefreshToken);
			secureTokensProvider.HashSecureToken(validRefreshToken).Returns(validRefreshTokenHash);
			secureTokensProvider.HashSecureToken(validPreviousRefreshToken).Returns(validPreviousRefreshTokenHash);
			secureTokensProvider.HashSecureToken(Arg.Is<string>(_ => _ != validRefreshToken && _ != validPreviousRefreshToken))
				.Throws(new FormatException("Invalid token"));
			secureTokensProvider.Validate(validRefreshTokenHash, validRefreshToken).Returns(true);
			secureTokensProvider.Validate(validPreviousRefreshTokenHash, validPreviousRefreshToken).Returns(true);
			secureTokensProvider
				.Validate(Arg.Is<string>(_=>_ != validRefreshTokenHash && _ != validPreviousRefreshTokenHash),
						  Arg.Is<string>(_ => _ != validRefreshToken && _ != validPreviousRefreshToken))
				.Returns(false);
			var config = new ConfigurationBuilder()
				.AddConfigObject(UserSessionOptions.KEY, new UserSessionOptions(1, "not_important"))
				.Build();
			var logger = Substitute.For<ILogger<DefaultUserSessionService>>();
			IJwtClaims jwtClaims = Substitute.For<IJwtClaims>();
			IJwtTokenProvider jwtTokenProvider = Substitute.For<IJwtTokenProvider>();
			jwtTokenProvider.GetToken(new Dictionary<string, string>()).ReturnsForAnyArgs(ServiceResult<string>.Success(validJwtAccessToken));
			return new DefaultUserSessionService(secureTokensProvider, config, logger, jwtTokenProvider, jwtClaims, db);
		}
	}
}
