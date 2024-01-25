namespace MicroTube.Data.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PublicUsername { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public AppUser(int id, string username, string email, string publicUsername, bool isEmailConfirmed)
        {
            Id = id;
            Username = username;
            Email = email;
            IsEmailConfirmed = isEmailConfirmed;
            PublicUsername = publicUsername;
        }
    }
}
