namespace MicroTube.Data.Models
{
	public class UsedRefreshToken
	{
		public int Id { get; set; }
		required public int SessionId { get; set; }
		required public string Token { get; set; }
		public AppUserSession? Session { get; set; }
	}
}
