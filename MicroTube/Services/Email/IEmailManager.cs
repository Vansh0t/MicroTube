namespace MicroTube.Services.Email
{
    public interface IEmailManager
    {
        public Task Send(string subject, string recipient, string htmlMessage);
        public Task SendMultiple(string subject, IEnumerable<string> recipients, string htmlMessage);
    }
}
