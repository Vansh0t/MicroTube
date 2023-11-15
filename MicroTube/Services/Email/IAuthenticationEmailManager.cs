namespace MicroTube.Services.Email
{
    public interface IAuthenticationEmailManager
    {
        public Task SendEmailConfirmation(string recipient, string data);
        public Task SendEmailChangeEnd(string recipient, string data);
        public Task SendPasswordResetStart(string recipient, string data);
        public Task SendPasswordResetEnd(string recipient, string data);
    }
}
