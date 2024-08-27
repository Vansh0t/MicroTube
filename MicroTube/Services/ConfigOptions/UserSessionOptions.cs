namespace MicroTube.Services.ConfigOptions
{
	public class UserSessionOptions
	{
		public const string KEY = "UserSession";

		public int TokenLifetimeMinutes { get; set; }
		public string RefreshPath { get; set; }
		public UserSessionOptions(int tokenLifetimeMinutes, string refreshPath)
		{
			TokenLifetimeMinutes = tokenLifetimeMinutes;
			RefreshPath = refreshPath;
		}
	}
}
