using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MicroTube.Services.Email;

namespace MicroTube.Tests.Integration.Email
{
    [Collection(nameof(AppTestsCollection))]
    public class EmailManager
    {
        private readonly MicroTubeWebAppFactory<Program> _appFactory;

        public EmailManager(MicroTubeWebAppFactory<Program> appFactory)
        {
            _appFactory = appFactory;
        }
        [Fact(Skip = "Skipped to prevent spam")]
        //[Fact]
        public async Task TestSendEmail()
        {
            using var scope = _appFactory.Services.CreateScope();
            var emailManager = scope.ServiceProvider.GetRequiredService<IEmailManager>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var emailOptions = config.GetRequiredByKey<EmailOptions>(EmailOptions.KEY);
            await emailManager.Send("Test Email", emailOptions.SMTP.Email, "<b>Test message</b>");
        }
    }
}
