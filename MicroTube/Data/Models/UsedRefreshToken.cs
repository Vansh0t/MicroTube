namespace MicroTube.Data.Models
{
	public class UsedRefreshToken
	{
		public Guid Id { get; set; }
		public Guid SessionId { get; set; }
		required public string Token { get; set; }
		public AppUserSession? Session { get; set; }
	}
}
