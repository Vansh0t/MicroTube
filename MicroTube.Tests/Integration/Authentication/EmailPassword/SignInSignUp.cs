using MicroTube.Controllers.Authentication.DTO;
using System.Net.Http.Json;

namespace MicroTube.Tests.Integration.Authentication.EmailPassword
{
    [Collection(nameof(AppTestsCollection))]
    public class SignInSignUp
    {
        private readonly MicroTubeWebAppFactory<Program> _appFactory;

        public SignInSignUp(MicroTubeWebAppFactory<Program> appFactory)
        {
            _appFactory = appFactory;
        }
        [Fact]
        public async Task TestSignUpSuccess()
        {
            var client = _appFactory.CreateClient();
            var result = await client.SignUpTestUser();
            Assert.NotNull(result.response);
            Assert.NotNull(result.response.JWT);
        }
        [Fact]
        public async Task TestSignInSuccess()
        {
            var client = _appFactory.CreateClient();
            var signUp = await client.SignUpTestUser();

            var response = await client.PostAsJsonAsync("authentication/emailpassword/signin", new SignInCredentialPasswordDTO(signUp.email, signUp.password));
            Assert.True(response.IsSuccessStatusCode);
            var responseDTO = await response.Content.ReadFromJsonAsync<AuthenticationResponseDTO>();
            Assert.NotNull(responseDTO);
            Assert.NotNull(responseDTO.JWT);
        }
    }
}
