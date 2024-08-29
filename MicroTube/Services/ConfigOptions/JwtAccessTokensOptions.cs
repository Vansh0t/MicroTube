namespace MicroTube.Services.ConfigOptions
{
	public class JwtAccessTokensOptions
	{
		public const string KEY = "JwtAccessTokens";
		public string Issuer { get; set; }
		public string Audience { get; set; }
		public int ExpirationMinutes { get; set; }
		public string Key { get; set; }
		public PasswordResetJwtAccessTokensOptions PasswordReset { get; set; }
		public JwtAccessTokensOptions(string issuer, string audience, int expirationMinutes, string key, PasswordResetJwtAccessTokensOptions passwordReset)
		{
			Issuer = issuer;
			Audience = audience;
			ExpirationMinutes = expirationMinutes;
			Key = key;
			PasswordReset = passwordReset;
		}
	}
}
