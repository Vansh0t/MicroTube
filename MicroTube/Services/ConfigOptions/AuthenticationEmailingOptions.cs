namespace MicroTube.Services.ConfigOptions
{
	public class AuthenticationEmailingOptions
	{
		public const string KEY = "AuthenticationEmailing";
		public string TemplatesLocation { get; set; }
		public string SenderSMTPUsername { get; set; }
		public string SenderAddress { get; set; }
		public string Sender { get; set; }
		public string SenderSMTPPassword { get; set; }
		public AuthenticationEmailingOptions(string templatesLocation, string senderAddress, string sender, string senderSMTPPassword, string senderSMTPUsername)
		{
			TemplatesLocation = templatesLocation;
			SenderAddress = senderAddress;
			Sender = sender;
			SenderSMTPPassword = senderSMTPPassword;
			SenderSMTPUsername = senderSMTPUsername;
		}
	}
}
