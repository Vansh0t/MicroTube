namespace MicroTube.Data.Models
{
	public class AppUserSession
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public required string Token { get; set; }
		public DateTime IssuedDateTime { get; set; }
		public DateTime ExpirationDateTime { get; set; }
		public bool IsInvalidated { get; set; }
		public List<UsedRefreshToken> UsedTokens { get; set; } = new List<UsedRefreshToken>();
	}
}
