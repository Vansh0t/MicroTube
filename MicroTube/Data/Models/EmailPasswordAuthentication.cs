namespace MicroTube.Data.Models
{
    public class EmailPasswordAuthentication
    {
		public Guid UserId { get; set; }
		public required string PasswordHash { get; set; }
        public string? EmailConfirmationString { get; set; }
        public DateTime? EmailConfirmationStringExpiration { get; set; }
        public string? PasswordResetString { get; set; }
        public DateTime? PasswordResetStringExpiration { get; set; }
        public string? PendingEmail { get; set; }
    }
}
