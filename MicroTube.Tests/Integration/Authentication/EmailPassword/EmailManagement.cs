using Microsoft.Extensions.DependencyInjection;
using MicroTube.Controllers.Authentication.DTO;
using MicroTube.Services.Email;
using MicroTube.Tests.Mocks;
using MicroTube.Tests.Utils;
using System.Net.Http.Json;

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
            var authEmailManager = (MockAuthenticationEmailManager)_appFactory.Services.GetRequiredService<IAuthenticationEmailManager>();
            await client.SignUpTestUser();
            Assert.NotNull(authEmailManager.SentEmailConfirmation);
            string url = $"authentication/EmailPassword/ConfirmEmail?emailConfirmationString={authEmailManager.SentEmailConfirmation}";
            var response = await client.GetAsync(url);
            Assert.True(response.IsSuccessStatusCode);
            var jwt = await response.Content.ReadFromJsonAsync<AuthenticationResponseDTO>();
            Assert.NotNull(jwt);
            var parsedJWT = Cryptography.ValidateAndGetClaimsFromJWTToken(jwt.JWT);
            var claim = parsedJWT["email_confirmed"];
            Assert.True(bool.Parse(claim));
        }
    }
}
