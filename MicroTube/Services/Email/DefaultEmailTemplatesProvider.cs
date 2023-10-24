namespace MicroTube.Services.Email
{
    public class DefaultEmailTemplatesProvider : IEmailTemplatesProvider
    {
        private const string ACTION_DATA_PLACEHOLDER = "{{actionData}}";
        private const string EMAIL_CONFIRMATION_TEMPLATE_FILE_NAME = "EmailConfirmation.html";
        private const string PASSWORD_RESET_TEMPLATE_FILE_NAME = "PasswordReset.html";

        private readonly IConfiguration _config;

        public DefaultEmailTemplatesProvider(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> BuildEmailConfirmationTemplate(string url)
        {
            var templateLocation = Path.Join(GetTemplatesLocation(), EMAIL_CONFIRMATION_TEMPLATE_FILE_NAME);
            if (!File.Exists(templateLocation))
                throw new RequiredObjectNotFoundException($"Unable to find email confirmation template at {templateLocation}");
            var result = await File.ReadAllTextAsync(templateLocation);
            return result.Replace(ACTION_DATA_PLACEHOLDER, url);
        }
        public async Task<string> BuildPasswordResetTemplate(string url)
        {
            var templateLocation = Path.Join(GetTemplatesLocation(), PASSWORD_RESET_TEMPLATE_FILE_NAME);
            if (!File.Exists(templateLocation))
                throw new RequiredObjectNotFoundException($"Unable to find password reset template at {templateLocation}");
            var result = await File.ReadAllTextAsync(templateLocation);
            return result.Replace(ACTION_DATA_PLACEHOLDER, url);
        }
        private string GetTemplatesLocation()
        {
            string? emailTemplatesLocation = _config["Email:TemplatesLocation"];
            if (emailTemplatesLocation == null)
                throw new ConfigurationException($"Configuration doesn't contain key for templates location Email:TemplatesLocation");
            return emailTemplatesLocation;
        }
    }
}
