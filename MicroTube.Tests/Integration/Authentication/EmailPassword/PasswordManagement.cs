using Microsoft.Extensions.DependencyInjection;
using MicroTube.Controllers.Authentication.DTO;
using MicroTube.Services.Cryptography;
using MicroTube.Services.Email;
using MicroTube.Tests.Mocks;
using MicroTube.Tests.Utils;
using System.Net.Http.Json;

namespace MicroTube.Tests.Integration.Authentication.EmailPassword
{
    [Collection(nameof(AppTestsCollection))]
    public class PasswordManagement
    {
        private readonly MicroTubeWebAppFactory<Program> _appFactory;

        public PasswordManagement(MicroTubeWebAppFactory<Program> appFactory)
        {
            _appFactory = appFactory;
        }
        [Fact]
        public async Task ChangePasswordSuccess()
        {
            string newPassword = Guid.NewGuid().ToString().Replace("-", "");
            var client = _appFactory.CreateClient();
            using var scope = _appFactory.Services.CreateScope();
            var authEmailManager = (MockAuthenticationEmailManager)scope.ServiceProvider.GetRequiredService<IAuthenticationEmailManager>();
            var passwordEncryption = scope.ServiceProvider.GetRequiredService<IPasswordEncryption>();
            var user = await client.SignUpTestUser();
            await TestDatabase.ConfirmEmail(user.username);

            ResetPasswordStartDTO resetPasswordStartData = new(user.email);
            string url = "authentication/EmailPassword/ResetPassword";
            var response = await client.PostAsJsonAsync(url, resetPasswordStartData);
            var content = await response.Content.ReadAsStringAsync();
            var dbUser = await TestDatabase.GetRequiredEmailPasswordUser(user.username);
            var passwordResetStringRaw = authEmailManager.SentPasswordResetStart;
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(dbUser.Authentication.PasswordResetString);
            Assert.NotNull(dbUser.Authentication.PasswordResetStringExpiration);
            Assert.True(dbUser.Authentication.PasswordResetStringExpiration > DateTime.UtcNow);
            Assert.NotNull(passwordResetStringRaw);
            Assert.False(passwordResetStringRaw == dbUser.Authentication.EmailConfirmationString);

            url = "authentication/EmailPassword/ValidatePasswordReset";
            response = await client.PostAsJsonAsync(url, passwordResetStringRaw);
            var passwordResetJWT = await response.Content.ReadFromJsonAsync<AuthenticationResponseDTO>();
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(passwordResetJWT);
            dbUser = await TestDatabase.GetRequiredEmailPasswordUser(user.username);
            Assert.Null(dbUser.Authentication.PasswordResetString);
            Assert.Null(dbUser.Authentication.PasswordResetStringExpiration);

            client.ApplyJWTBearer(passwordResetJWT.JWT);
            url = "authentication/EmailPassword/ChangePassword";
            response = await client.PostAsJsonAsync(url, new PasswordChangeDTO(newPassword, newPassword));
            dbUser = await TestDatabase.GetRequiredEmailPasswordUser(user.username);
            bool isNewPasswordValid = passwordEncryption.Validate(dbUser.Authentication.PasswordHash, newPassword);
            Assert.True(response.IsSuccessStatusCode);
            Assert.True(isNewPasswordValid);
            Assert.Null(dbUser.Authentication.PasswordResetString);
            Assert.Null(dbUser.Authentication.PasswordResetStringExpiration);

        }
        
    }
}
