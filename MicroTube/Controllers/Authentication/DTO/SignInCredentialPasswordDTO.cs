using MicroTube.Data;

namespace MicroTube.Controllers.Authentication.DTO
{
    public class SignInCredentialPasswordDTO
    {
        public string Credential { get; set; }
        public string Password { get; set; }
        public SignInCredentialPasswordDTO(string credential, string password)
        {
            Credential = credential;
            Password = password;
        }
    }
}
