namespace MicroTube.Controllers.Authentication.Dto
{
    public class PasswordChangeDto
    {
        public string NewPassword { get; set; }
        public PasswordChangeDto(string newPassword)
        {
            NewPassword = newPassword;
        }
    }
}
