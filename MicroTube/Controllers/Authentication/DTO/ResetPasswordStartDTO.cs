namespace MicroTube.Controllers.Authentication.DTO
{
    public class ResetPasswordStartDTO
    {
        public string Email { get; set; }
        public ResetPasswordStartDTO(string email)
        {
            Email = email;
        }

    }
}
