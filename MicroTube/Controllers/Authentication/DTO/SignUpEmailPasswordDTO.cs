using MicroTube.Data;

namespace MicroTube.Controllers.Authentication.Dto
{
    public class SignUpEmailPasswordDto
    { 
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public SignUpEmailPasswordDto(string username, string email, string password)
        {
            Username = username;
            Email = email;
            Password = password;
        }
    }
}
