namespace MicroTube.Services.Email
{
	public class SenderCredentials
	{
		public required string Name { get; set; }
		public required string Address { get; set; }
		public required string SMTPUsername { get; set; }
		public required string SMTPPassword { get; set; }
	}
}
