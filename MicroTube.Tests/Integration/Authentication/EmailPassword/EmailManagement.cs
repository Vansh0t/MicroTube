namespace MicroTube.Tests.Integration.Authentication.EmailPassword
{
    [Collection(nameof(AppTestsCollection))]
    public class EmailManagement
    {
        private readonly MicroTubeWebAppFactory<Program> _appFactory;

        public EmailManagement(MicroTubeWebAppFactory<Program> appFactory)
        {
            _appFactory = appFactory;
        }
        [Fact]
        public async Task ConfirmEmailSuccess()
        {
            var client = _appFactory.CreateClient();
            var user = await client.SignUpTestUser();
        }
    }
}
