using MimeKit;
using MailKit.Net.Smtp;
using System.Net.NetworkInformation;
using MimeKit.Utils;

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

        public async Task Send(string subject, string recipient, string htmlMessage)
        {
            EmailOptions options = _config.GetRequiredByKey<EmailOptions>(EmailOptions.KEY);
            var message = BuildMessage(subject, recipient, htmlMessage, options);

            using var client = new SmtpClient();
            client.LocalDomain = options.SMTP.SenderDomain;
            await client.ConnectAsync(options.SMTP.Server, options.SMTP.TLSPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(options.SMTP.Email, options.SMTP.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendMultiple(string subject, IEnumerable<string> recipients, string htmlMessage)
        {
            EmailOptions options = _config.GetRequiredByKey<EmailOptions>(EmailOptions.KEY);
            using var client = new SmtpClient();
            await client.ConnectAsync(options.SMTP.Server);
            await client.AuthenticateAsync(options.SMTP.Email, options.SMTP.Password);
            foreach (var recipient in recipients)
            {
                try
                {
                    var message = BuildMessage(subject, recipient, htmlMessage, options);
                    await client.SendAsync(message);
                }
                catch(Exception e)
                {
                    _logger.LogError(e, $"Failed to send an email to {recipient}");
                }
            }
            await client.DisconnectAsync(true);
        }
        private MimeMessage BuildMessage(string subject, string recipient, string htmlMessage, EmailOptions emailOptions)
        {
            var message = new MimeMessage();
            message.MessageId = MimeUtils.GenerateMessageId(emailOptions.SMTP.SenderDomain);
            message.From.Add(new MailboxAddress(emailOptions.Sender, emailOptions.SenderAddress));
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
