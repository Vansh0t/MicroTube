namespace MicroTube.Controllers.Authentication.DTO
{
    public class EmailChangeDTO
    {
        public string NewEmail { get; set; }
        public string Password { get; set; }
        public EmailChangeDTO(string newEmail, string password)
        {
            NewEmail = newEmail;
            Password = password;
        }

    }
}
