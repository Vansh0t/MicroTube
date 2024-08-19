namespace MicroTube.Data.Models
{
	public class AppUserSession
	{
		public required Guid Id { get; set; }
		public required Guid UserId { get; set; }
		public required string Token { get; set; }
		public required DateTime IssuedDateTime { get; set; }
		public required DateTime ExpirationDateTime { get; set; }
		public required bool IsInvalidated { get; set; }
		public IEnumerable<UsedRefreshToken> UsedTokens { get; set; } = new List<UsedRefreshToken>();
	}
}
