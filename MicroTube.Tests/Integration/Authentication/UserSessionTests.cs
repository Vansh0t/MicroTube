using Microsoft.Extensions.DependencyInjection;
using MicroTube.Controllers.Authentication.DTO;
using MicroTube.Data.Access;
using MicroTube.Tests.Utils;
using System.Net;
using System.Net.Http.Json;

namespace MicroTube.Tests.Integration.Authentication
{
	[Collection(nameof(AppTestsCollection))]
	public class UserSessionTests
	{
		private readonly MicroTubeWebAppFactory<Program> _appFactory;

		public UserSessionTests(MicroTubeWebAppFactory<Program> appFactory)
		{
			_appFactory = appFactory;
		}
		[Fact]
		public async Task RefreshSession_Success()
		{
			var client = _appFactory.CreateClient();
			client.BaseAddress = new Uri("https://localhost");
			var user = await client.SignUpTestUser();

			var response1 = await client.PostAsync("Authentication/Session/Refresh", null);
			response1.EnsureSuccessStatusCode();
			AuthenticationResponseDTO? content1 = await response1.Content.ReadFromJsonAsync<AuthenticationResponseDTO>();
			Assert.NotNull(content1);
			Assert.False(string.IsNullOrWhiteSpace(content1.JWT));

			var cookieKeyValues = response1.GetSetCookieKeyValues();
			Assert.NotEmpty(cookieKeyValues);
			var refreshCookie1 = cookieKeyValues.FirstOrDefault(_ => _.Name == Constants.AuthorizationConstants.REFRESH_TOKEN_COOKIE_KEY);
			Assert.NotNull(refreshCookie1);
			Assert.False(string.IsNullOrWhiteSpace(refreshCookie1.Value));
			await Task.Delay(1200);//let a bit of time to pass so access tokens will be different

			var response2 = await client.PostAsync("Authentication/Session/Refresh", null);
			response2.EnsureSuccessStatusCode();
			AuthenticationResponseDTO? content2 = await response2.Content.ReadFromJsonAsync<AuthenticationResponseDTO>();
			Assert.NotNull(content2);
			Assert.False(string.IsNullOrWhiteSpace(content2.JWT));
			Assert.NotEqual(content2.JWT, content1.JWT);

			cookieKeyValues = response2.GetSetCookieKeyValues();
			Assert.NotEmpty(cookieKeyValues);
			var refreshCookie2 = cookieKeyValues.FirstOrDefault(_ => _.Name == Constants.AuthorizationConstants.REFRESH_TOKEN_COOKIE_KEY);
			Assert.NotNull(refreshCookie2);
			Assert.False(string.IsNullOrWhiteSpace(refreshCookie2.Value));
			Assert.NotEqual(refreshCookie1.Value, refreshCookie2.Value);
		}
		[Fact]
		public async Task RefreshSession_InvalidationDueToTokenRotationViolation()
		{
			IUserSessionDataAccess sessionDataAccess = _appFactory.Services.GetRequiredService<IUserSessionDataAccess>();
			IAppUserDataAccess userDataAccess = _appFactory.Services.GetRequiredService<IAppUserDataAccess>();
			var client = _appFactory.CreateClient();
			client.BaseAddress = new Uri("https://localhost");
			var signInUser = await client.SignUpTestUser();
			var user = await userDataAccess.GetByUsername(signInUser.username);
			Assert.NotNull(user);
			string userId = user.Id.ToString();
			var refreshResponse = await client.PostAsync("Authentication/Session/Refresh", null);
			var cookieKeyValues = refreshResponse.GetSetCookieKeyValues();
			var prevRefreshToken = cookieKeyValues.First(_ => _.Name == Constants.AuthorizationConstants.REFRESH_TOKEN_COOKIE_KEY).Value;
			await Task.Delay(500);
			var refreshResponse1 = await client.PostAsync("Authentication/Session/Refresh", null);
			refreshResponse.EnsureSuccessStatusCode();
			refreshResponse1.EnsureSuccessStatusCode();
			var session = await TestDatabase.GetUserSession(user.Id.ToString());
			Assert.NotNull(session);
			Assert.Equal(2, session.UsedTokens.Count);

			var cookieContainer = new CookieContainer();
			using var httpHandler = new HttpClientHandler { CookieContainer = cookieContainer };
			using var otherClient = _appFactory.CreateClient();
			var otherRefreshTokenCookie = new Cookie(Constants.AuthorizationConstants.REFRESH_TOKEN_COOKIE_KEY, prevRefreshToken, "/Authentication/Session/Refresh", "localhost");
			var str = otherRefreshTokenCookie.ToString();
			otherClient.DefaultRequestHeaders.Add("Cookie", otherRefreshTokenCookie.ToString());
			var otherRefreshResponse = await otherClient.PostAsync("Authentication/Session/Refresh", null);
			Assert.False(otherRefreshResponse.IsSuccessStatusCode);
			Assert.Equal(403, (int)otherRefreshResponse.StatusCode);
			Assert.ThrowsAny<Exception>(()=> otherRefreshResponse.GetSetCookieKeyValues());
			session = await TestDatabase.GetUserSession(user.Id.ToString());
			Assert.NotNull(session);
			Assert.True(session.IsInvalidated);
			var legitRefreshAfterInvalidation = await client.PostAsync("Authentication/Session/Refresh", null);
			Assert.False(legitRefreshAfterInvalidation.IsSuccessStatusCode);
			Assert.Equal(403, (int)legitRefreshAfterInvalidation.StatusCode);
		}
	}
}
