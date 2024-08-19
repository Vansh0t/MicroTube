namespace MicroTube.Data.Models
{
    public class AppUser
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string PublicUsername { get; set; }
        public required bool IsEmailConfirmed { get; set; }
		public IAuthenticationData? Authentication { get; set; }
    }
}
