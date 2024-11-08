namespace MicroTube.Controllers.Authentication.Dto
{
    public class ResetPasswordStartDto
    {
        public string Email { get; set; }
        public ResetPasswordStartDto(string email)
        {
            Email = email;
        }

    }
}
