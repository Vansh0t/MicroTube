using MicroTube.Data;

namespace MicroTube.Controllers.Authentication.Dto
{
    public class SignInCredentialPasswordDto
    {
        public string Credential { get; set; }
        public string Password { get; set; }
        public SignInCredentialPasswordDto(string credential, string password)
        {
            Credential = credential;
            Password = password;
        }
    }
}
