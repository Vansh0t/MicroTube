namespace MicroTube.Data.Models
{
	public class UsedRefreshToken
	{
		public Guid Id { get; set; }
		public required Guid SessionId { get; set; }
		public AppUserSession? Session { get; set; }
		public required string Token { get; set; }
	}
}
