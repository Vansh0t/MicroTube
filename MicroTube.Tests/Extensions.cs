using Microsoft.Extensions.Configuration;
using System.Data;
using Dapper;
using System.Net.Http.Json;
using MicroTube.Controllers.Authentication.DTO;

namespace MicroTube.Tests
{
    public static class Extensions
    {
        private const string DACPAC_PATH_NAME = "DacPac:Path";
        public static string GetDacPacPath(this IConfiguration configuration)
        {
            string? defaultConnectionString = configuration[DACPAC_PATH_NAME];
            if (defaultConnectionString is null)
                throw new ConfigurationException("Configuration does not contain dacpac file path " + DACPAC_PATH_NAME);
            return defaultConnectionString;
        }
        public static void DropTestDatabase(this IDbConnection connection)
        {
            connection.Execute($"DROP DATABASE [{connection.Database}] IF EXISTS");
        }
        public static void CreateTestDatabase(this IDbConnection connection)
        {
            connection.Execute($"CREATE DATABASE [{connection.Database}]");
        }
        public static void RecreateTestDatabase(this IDbConnection connection)
        {
            connection.DropTestDatabase();
            connection.CreateTestDatabase();
        }
        public static async Task<(string email, string username, string password, AuthenticationResponseDTO response)> SignUpTestUser(this HttpClient client)
        {
            string password = Guid.NewGuid().ToString().Replace("-", "");
            string email = Guid.NewGuid().ToString().Replace("-", "") + "@email.com";
            string username = Guid.NewGuid().ToString().Replace("-", "").Remove(0,8);
            var response = await client.PostAsJsonAsync("authentication/EmailPassword/SignUp", new SignUpEmailPasswordDTO(username, email, password));
            if(!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception(error + " " + email + " " + username + " " + password);
            }
            var responseContent = await response.Content.ReadFromJsonAsync<AuthenticationResponseDTO>();
            if(responseContent == null)
            {
                throw new Exception("Response content is null");
            }
            return (email, username, password, responseContent); 
        }
        public static async Task<AuthenticationResponseDTO> SignInTestUser(this HttpClient client, string credential, string password)
        {
            var response = await client.PostAsJsonAsync("authentication/EmailPassword/SignIn", new SignInCredentialPasswordDTO(credential, password));
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception(error + " " + credential + " " + password);
            }
            var responseContent = await response.Content.ReadFromJsonAsync<AuthenticationResponseDTO>();
            if (responseContent == null)
            {
                throw new Exception("Response content is null");
            }
            return responseContent;
        }
        public static void ApplyJWTBearer(this HttpClient client, string jwtToken)
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
        }
    }
}
