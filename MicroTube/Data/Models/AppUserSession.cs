namespace MicroTube.Data.Models
{
	public class AppUserSession
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string Token { get; set; }
		public string? PreviousToken { get; set; }
		public DateTime IssuedDateTime { get; set; }
		public DateTime ExpirationDateTime { get; set; }
		public bool IsInvalidated { get; set; }
		public AppUserSession(int id, int userId, string token, string? previousToken, DateTime issuedDateTime, DateTime expirationDateTime, bool isInvalidated)
		{
			Id = id;
			UserId = userId;
			Token = token;
			PreviousToken = previousToken;
			IssuedDateTime = issuedDateTime;
			ExpirationDateTime = expirationDateTime;
			IsInvalidated = isInvalidated;
		}
	}
}
