using MicroTube.Services.ConfigOptions;

namespace MicroTube.Services.Email
{
    public class DefaultAuthenticationEmailManager : IAuthenticationEmailManager
    {
        private const string EMAIL_CONFIRMATION_QUERY_STRING = "emailConfirmationString";
        private const string PASSWORD_RESET_QUERY_STRING = "passwordResetString";

        private readonly ILogger<DefaultAuthenticationEmailManager> _logger;
        private readonly IEmailManager _emailManager;
        private readonly IEmailTemplatesProvider _templatesProvider;
        private readonly IConfiguration _config;

        public DefaultAuthenticationEmailManager(
            ILogger<DefaultAuthenticationEmailManager> logger,
            IEmailManager emailManager,
            IEmailTemplatesProvider templatesProvider,
            IConfiguration config)
        {
            _logger = logger;
            _emailManager = emailManager;
            _templatesProvider = templatesProvider;
            _config = config;
        }

        public async Task SendEmailChangeEnd(string recipient, string data)
        {
			SenderCredentials sender = BuildAuthSenderCredentials();
			await _emailManager.Send(sender, "Email Changed", recipient, "<b>Email changed successfully.</b>");
        }
        public async Task SendEmailConfirmation(string recipient, string data)
        {
			SenderCredentials sender = BuildAuthSenderCredentials();
			var options = _config.GetRequiredByKey<EmailPasswordAuthEndpointsOptions>(EmailPasswordAuthEndpointsOptions.KEY);
            var uriBuilder = new UriBuilder(options.EmailConfirmation);
            uriBuilder.Query = $"{EMAIL_CONFIRMATION_QUERY_STRING}={data}";
            var url = uriBuilder.ToString();
            var template = await _templatesProvider.BuildEmailConfirmationTemplate(url);
            await _emailManager.Send(sender, "Email Confirmation", recipient, template);
        }

        public async Task SendPasswordResetEnd(string recipient, string data)
        {
			SenderCredentials sender = BuildAuthSenderCredentials();
			await _emailManager.Send(sender, "Password Changed", recipient, "<b>Password changed successfully.</b>");
        }

        public async Task SendPasswordResetStart(string recipient, string data)
        {
			SenderCredentials sender = BuildAuthSenderCredentials();
			var options = _config.GetRequiredByKey<EmailPasswordAuthEndpointsOptions>(EmailPasswordAuthEndpointsOptions.KEY);
            var uriBuilder = new UriBuilder(options.PasswordReset);
            uriBuilder.Query = $"{PASSWORD_RESET_QUERY_STRING}={data}";
            var url = uriBuilder.ToString();
            var template = await _templatesProvider.BuildPasswordResetTemplate(url);
            await _emailManager.Send(sender, "Password Reset Requested", recipient, template);
        }
		private SenderCredentials BuildAuthSenderCredentials()
		{
			var options = _config.GetRequiredByKey<AuthenticationEmailingOptions>(AuthenticationEmailingOptions.KEY);
			return new SenderCredentials { Address = options.SenderAddress, Name = options.Sender, SMTPUsername = options.SenderSMTPUsername, SMTPPassword = options.SenderSMTPPassword };
		}
    }
}
