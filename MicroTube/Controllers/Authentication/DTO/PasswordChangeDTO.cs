namespace MicroTube.Controllers.Authentication.DTO
{
    public class PasswordChangeDTO
    {
        public string NewPassword { get; set; }
        public PasswordChangeDTO(string newPassword)
        {
            NewPassword = newPassword;
        }
    }
}
