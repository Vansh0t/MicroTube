using MicroTube.Services.ConfigOptions;
using System.IO.Abstractions;

namespace MicroTube.Services.Email
{
    public class DefaultEmailTemplatesProvider : IEmailTemplatesProvider
    {
        private const string ACTION_DATA_PLACEHOLDER = "{{actionData}}";
        private const string EMAIL_CONFIRMATION_TEMPLATE_FILE_NAME = "EmailConfirmation.html";
        private const string PASSWORD_RESET_TEMPLATE_FILE_NAME = "PasswordReset.html";

        private readonly IConfiguration _config;
		private readonly IFileSystem _fileSystem;

		public DefaultEmailTemplatesProvider(IConfiguration config, IFileSystem fileSystem)
		{
			_config = config;
			_fileSystem = fileSystem;
		}

		public async Task<string> BuildEmailConfirmationTemplate(string url)
        {
            var templateLocation = _fileSystem.Path.Join(_config.GetAppRootPath(), GetTemplatesLocation(), EMAIL_CONFIRMATION_TEMPLATE_FILE_NAME);
            if (!_fileSystem.File.Exists(templateLocation))
                throw new RequiredObjectNotFoundException($"Unable to find email confirmation template at {templateLocation}");
            var result = await _fileSystem.File.ReadAllTextAsync(templateLocation);
            return result.Replace(ACTION_DATA_PLACEHOLDER, url);
        }
        public async Task<string> BuildPasswordResetTemplate(string url)
        {
            var templateLocation = _fileSystem.Path.Join(_config.GetAppRootPath(), GetTemplatesLocation(), PASSWORD_RESET_TEMPLATE_FILE_NAME);
            if (!File.Exists(templateLocation))
                throw new RequiredObjectNotFoundException($"Unable to find password reset template at {templateLocation}");
            var result = await _fileSystem.File.ReadAllTextAsync(templateLocation);
            return result.Replace(ACTION_DATA_PLACEHOLDER, url);
        }
        private string GetTemplatesLocation()
        {
			var options = _config.GetRequiredByKey<AuthenticationEmailingOptions>(AuthenticationEmailingOptions.KEY);
			string emailTemplatesLocation = options.TemplatesLocation;
			return emailTemplatesLocation;
        }
    }
}
