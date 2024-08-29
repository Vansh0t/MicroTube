namespace MicroTube.Services.ConfigOptions
{
	public class PasswordResetJwtAccessTokensOptions
	{
		public int ExpirationMinutes { get; set; }
		public PasswordResetJwtAccessTokensOptions(int expirationMinutes)
		{
			ExpirationMinutes = expirationMinutes;
		}

	}
}
