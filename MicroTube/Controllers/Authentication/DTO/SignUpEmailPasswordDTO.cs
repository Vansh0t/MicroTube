using MicroTube.Data;

namespace MicroTube.Controllers.Authentication.DTO
{
    public class SignUpEmailPasswordDTO
    { 
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public SignUpEmailPasswordDTO(string username, string email, string password)
        {
            Username = username;
            Email = email;
            Password = password;
        }
    }
}
