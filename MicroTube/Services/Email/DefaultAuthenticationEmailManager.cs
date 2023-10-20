namespace MicroTube.Services.Email
{
    public class DefaultAuthenticationEmailManager : IAuthenticationEmailManager
    {
        private readonly ILogger<DefaultAuthenticationEmailManager> _logger;
        private readonly IEmailManager _emailManager;

        public DefaultAuthenticationEmailManager(ILogger<DefaultAuthenticationEmailManager> logger, IEmailManager emailManager)
        {
            _logger = logger;
            _emailManager = emailManager;
        }

        public Task SendEmailChangeEnd(string recipient, string data)
        {
            throw new NotImplementedException();
        }

        public Task SendEmailChangeStart(string recipient, string data)
        {
            throw new NotImplementedException();
        }

        public Task SendEmailConfirmation(string recipient, string data)
        {
            throw new NotImplementedException();
        }

        public Task SendPasswordResetEnd(string recipient, string data)
        {
            throw new NotImplementedException();
        }

        public Task SendPasswordResetStart(string recipient, string data)
        {
            throw new NotImplementedException();
        }
    }
}
