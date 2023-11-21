using MicroTube.Controllers.Authentication.DTO;
using System.Net;
using System.Net.Http.Json;

namespace MicroTube.Tests.Integration.Authentication
{
	[Collection(nameof(AppTestsCollection))]
	public class UserSession
	{
		private readonly MicroTubeWebAppFactory<Program> _appFactory;

		public UserSession(MicroTubeWebAppFactory<Program> appFactory)
		{
			_appFactory = appFactory;
		}
		[Fact]
		public async Task RefreshSessionSuccess()
		{
			var client = _appFactory.CreateClient();
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
	}
}
