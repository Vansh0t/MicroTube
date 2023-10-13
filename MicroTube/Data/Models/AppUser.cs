namespace MicroTube.Data.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public AppUser(int id, string username, string email)
        {
            Id = id;
            Username = username;
            Email = email;
        }
    }
}
