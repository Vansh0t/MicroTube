namespace MicroTube.Services.Email
{
    public interface IEmailManager
    {
        public Task Send(string recipient, string message);
        public Task SendMultiple(IEnumerable<string> recipients, string message);
    }
}
