using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MicroTube.Data.Models;
using System.Data;

namespace MicroTube.Tests.Utils
{
    public static class TestDatabase
    {
        private const string DATABASE_NAME = "MicroTubeDb.Tests";
        public static string Create()
        {
            //Drop();
            IConfiguration config = ConfigurationProvider.GetConfiguration();
            string databaseName = DATABASE_NAME;
            DacPac.DeployDacPac(config.GetDefaultConnectionString(), config.GetDacPacPath(), databaseName);
            return databaseName;
        }
        public static void Drop()
        {
            IConfiguration config = ConfigurationProvider.GetConfiguration();
            using IDbConnection connection = new SqlConnection(config.GetDefaultConnectionString());
            connection.Execute($"USE master;" +
                $"ALTER DATABASE [{DATABASE_NAME}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;" +
                $"DROP DATABASE IF EXISTS [{DATABASE_NAME}]");
        }
        public static async Task ConfirmEmail(string username)
        {
            using IDbConnection connection = new SqlConnection(ConfigurationProvider.GetConfiguration().GetDefaultConnectionString());
            string sql = @"UPDATE dbo.AppUser
	                       SET IsEmailConfirmed = 1
	                       WHERE Username = @Username;";
            await connection.ExecuteAsync(sql, new { Username = username });
        }
        public static async Task<AppUser?> GetUser(string username)
        {
            using IDbConnection connection = new SqlConnection(ConfigurationProvider.GetConfiguration().GetDefaultConnectionString());
            string sql = @"SELECT * FROM dbo.AppUser
	                       WHERE Username = @Username;";
            return await connection.QueryFirstOrDefaultAsync(sql, new { Username = username });
        }
        public static async Task<AppUser> GetRequiredUser(string username)
        {
            var user = await GetUser(username);
            if (user == null)
                throw new RequiredObjectNotFoundException($"User {username} was not found");
            return user;
        }
        public static async Task<EmailPasswordAppUser?> GetEmailPasswordUser(string username)
        {
            using IDbConnection connection = new SqlConnection(ConfigurationProvider.GetConfiguration().GetDefaultConnectionString());
            string sql = @"SELECT a.*, u.*
	                       FROM dbo.EmailPasswordAuthentication a
	                       INNER JOIN dbo.AppUser u 
	                       ON a.UserId = u.Id
	                       AND u.Username = @Username;";
            var result = await connection.QueryAsync<EmailPasswordAuthentication, AppUser, EmailPasswordAppUser>(sql, (a, u)=> 
            {
                return new EmailPasswordAppUser(u, a);
            }, new { Username = username });
            return result.FirstOrDefault();
        }
        public static async Task<EmailPasswordAppUser> GetRequiredEmailPasswordUser(string username)
        {
            var user = await GetEmailPasswordUser(username);
            if (user == null)
                throw new RequiredObjectNotFoundException($"User {username} was not found");
            return user;
        }
    }
}
