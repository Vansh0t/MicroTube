using MimeKit;
using MailKit.Net.Smtp;
using MimeKit.Utils;
using MicroTube.Services.ConfigOptions;

namespace MicroTube.Services.Email
{
    public class DefaultEmailManager : IEmailManager
    {
        private readonly ILogger<DefaultEmailManager> _logger;
        private readonly IConfiguration _config;

        public DefaultEmailManager(ILogger<DefaultEmailManager> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task Send(SenderCredentials sender, string subject, string recipient, string htmlMessage)
        {
            SMTPOptions options = _config.GetRequiredByKey<SMTPOptions>(SMTPOptions.KEY);
            var message = BuildMessage(sender, subject, recipient, htmlMessage, options);

            using var client = new SmtpClient();
            client.LocalDomain = options.Domain;
            await client.ConnectAsync(options.Server, options.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(sender.SMTPUsername, sender.SMTPPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        private MimeMessage BuildMessage(SenderCredentials sender, string subject, string recipient, string htmlMessage, SMTPOptions options)
        {
            var message = new MimeMessage();
            message.MessageId = MimeUtils.GenerateMessageId(options.Domain);
            message.From.Add(new MailboxAddress(sender.Name, sender.Address));
            message.To.Add(new MailboxAddress(recipient, recipient));

            message.Subject = subject;
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlMessage
            };
            return message;
        }
    }
}
