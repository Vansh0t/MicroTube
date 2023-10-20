namespace MicroTube.Services.Email
{
    public class DefaultEmailManager : IEmailManager
    {
        public Task Send(string recipient, string message)
        {
            throw new NotImplementedException();
        }

        public Task SendMultiple(IEnumerable<string> recipients, string message)
        {
            throw new NotImplementedException();
        }
    }
}
