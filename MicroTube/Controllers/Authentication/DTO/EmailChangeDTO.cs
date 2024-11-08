namespace MicroTube.Controllers.Authentication.Dto
{
    public class EmailChangeDto
    {
        public string NewEmail { get; set; }
        public string Password { get; set; }
        public EmailChangeDto(string newEmail, string password)
        {
            NewEmail = newEmail;
            Password = password;
        }

    }
}
