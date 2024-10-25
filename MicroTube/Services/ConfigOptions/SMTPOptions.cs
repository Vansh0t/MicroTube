namespace MicroTube.Services.ConfigOptions
{
	public class SMTPOptions
	{
		public const string KEY = "SMTP";
		public string Server { get; set; }
		public int Port { get; set; }
		public string Domain { get; set; }
		public SMTPOptions(string server, int port, string domain)
		{
			Server = server;
			Port = port;
			Domain = domain;
		}
	}
}
