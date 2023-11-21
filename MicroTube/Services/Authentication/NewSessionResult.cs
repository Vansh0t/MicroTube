using MicroTube.Data.Models;

namespace MicroTube.Services.Authentication
{
	public class NewSessionResult
	{
		public AppUserSession Session { get; set; }
		public string RefreshTokenRaw { get; set; }
		public string AccessToken { get; set; }
		public NewSessionResult(AppUserSession session, string refreshTokenRaw, string accessToken)
		{
			Session = session;
			RefreshTokenRaw = refreshTokenRaw;
			AccessToken = accessToken;
		}
	}
}
