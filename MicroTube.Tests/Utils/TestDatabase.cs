using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
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
    }
}
