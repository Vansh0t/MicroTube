using Dapper;
using Microsoft.Data.SqlClient;
using MicroTube.Data.Models;
using System.Data;

namespace MicroTube.Data.Access.SQLServer
{
    public class AppUserDataAccess : IAppUserDataAccess
    {
        private const string SP_GET = "dbo.AppUser_Get";

        private readonly IConfiguration _configuration;
        private readonly ILogger<AppUserDataAccess> _logger;
        public AppUserDataAccess(IConfiguration configuration, ILogger<AppUserDataAccess> logger)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<AppUser?> Get(int id)
        {
            using IDbConnection connection = new SqlConnection(_configuration.GetDefaultConnectionString());
            var parameters = new
            {
                Id = id
            };
            var result = await connection.QueryAsync<AppUser>(
                SP_GET, parameters, commandType: CommandType.StoredProcedure);
            return result.FirstOrDefault();
        }
    }
}
