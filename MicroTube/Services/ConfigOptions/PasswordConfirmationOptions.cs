namespace MicroTube.Services.ConfigOptions
{
	public class PasswordConfirmationOptions
	{
		public const string KEY = "PasswordConfirmation";

		public int ExpirationSeconds { get; set; }
		public int StringLength { get; set; }

		public PasswordConfirmationOptions(int expirationSeconds, int stringLength)
		{
			ExpirationSeconds = expirationSeconds;
			StringLength = stringLength;
		}
	}
}
