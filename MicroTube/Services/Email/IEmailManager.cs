namespace MicroTube.Services.Email
{
    public interface IEmailManager
    {
        public Task Send(SenderCredentials sender, string subject, string recipient, string htmlMessage);
    }
}
