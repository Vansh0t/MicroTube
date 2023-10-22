﻿using Microsoft.Extensions.DependencyInjection;
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
        [Fact]
        public async Task ChangeEmailSuccess()
        {
            string newEmail = Guid.NewGuid().ToString().Replace("-", "")+"@email.com";
            var client = _appFactory.CreateClient();
            var authEmailManager = (MockAuthenticationEmailManager)_appFactory.Services.GetRequiredService<IAuthenticationEmailManager>();
            var user = await client.SignUpTestUser();
            await TestDatabase.ConfirmEmail(user.username);
            var signInResult = await client.SignInTestUser(user.email, user.password);
            client.ApplyJWTBearer(signInResult.JWT);

            EmailChangeDTO changeEmailContent = new EmailChangeDTO(newEmail, user.password);
            string url = "authentication/EmailPassword/ChangeEmailStart";
            var response = await client.PostAsJsonAsync(url, changeEmailContent);
            //var content = await response.Content.ReadAsStringAsync();
            Assert.True(response.IsSuccessStatusCode);

            var emailConfirmationStringRaw = authEmailManager.SentEmailChangeStart;
            var dbUser = await TestDatabase.GetRequiredEmailPasswordUser(user.username);
            Assert.NotNull(emailConfirmationStringRaw);
            Assert.NotNull(dbUser.Authentication.EmailConfirmationString);
            Assert.NotNull(dbUser.Authentication.EmailConfirmationStringExpiration);
            Assert.True(dbUser.Authentication.EmailConfirmationStringExpiration > DateTime.UtcNow);
            Assert.False(emailConfirmationStringRaw == dbUser.Authentication.EmailConfirmationString);

            url = $"authentication/EmailPassword/ChangeEmailConfirm?emailChangeConfirmationString={emailConfirmationStringRaw}";
            response = await client.GetAsync(url);
            //content = await response.Content.ReadAsStringAsync();
            Assert.True(response.IsSuccessStatusCode);
            dbUser = await TestDatabase.GetRequiredEmailPasswordUser(user.username);
            Assert.Null(dbUser.Authentication.EmailConfirmationString);
            Assert.Null(dbUser.Authentication.EmailConfirmationStringExpiration);
            Assert.Null(dbUser.Authentication.PendingEmail);
            Assert.Equal(newEmail, dbUser.Email);

        }
        
    }
}
