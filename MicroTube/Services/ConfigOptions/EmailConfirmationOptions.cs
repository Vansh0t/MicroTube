namespace MicroTube.Services.ConfigOptions
{
	public class EmailConfirmationOptions
	{
		public const string KEY = "EmailConfirmation";

		public int ExpirationSeconds { get; set; }
		public int StringLength { get; set; }

		public EmailConfirmationOptions(int expirationSeconds, int stringLength)
		{
			ExpirationSeconds = expirationSeconds;
			StringLength = stringLength;
		}
	}
}
