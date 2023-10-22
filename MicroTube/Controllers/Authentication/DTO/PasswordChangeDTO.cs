namespace MicroTube.Controllers.Authentication.DTO
{
    public class PasswordChangeDTO
    {
        public string NewPassword { get; set; }
        public string ConfirmedNewPassword { get; set; }
        public PasswordChangeDTO(string newPassword, string confirmedNewPassword)
        {
            NewPassword = newPassword;
            ConfirmedNewPassword = confirmedNewPassword;
        }
    }
}
