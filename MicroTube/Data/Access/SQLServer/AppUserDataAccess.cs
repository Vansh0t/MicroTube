using Dapper;
using Microsoft.Data.SqlClient;
using MicroTube.Data.Models;
using System.Data;

namespace MicroTube.Data.Access.SQLServer
{
    public class AppUserDataAccess : IAppUserDataAccess
    {
        private const string SP_GET = "dbo.AppUser_Get";
        private const string SP_GET_BY_EMAIL = "dbo.AppUser_GetByEmail";
        private const string SP_GET_BY_USERNAME = "dbo.AppUser_GetByUsername";

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
            var result = await connection.QueryFirstOrDefaultAsync<AppUser>(
                SP_GET, parameters, commandType: CommandType.StoredProcedure);
            return result;
        }

        public async Task<AppUser?> GetByEmail(string email)
        {
            using IDbConnection connection = new SqlConnection(_configuration.GetDefaultConnectionString());
            var parameters = new
            {
                Email = email
            };
            var result = await connection.QueryFirstOrDefaultAsync<AppUser>(
                SP_GET_BY_EMAIL, parameters, commandType: CommandType.StoredProcedure);
            return result;
        }

        public async Task<AppUser?> GetByUsername(string username)
        {
            using IDbConnection connection = new SqlConnection(_configuration.GetDefaultConnectionString());
            var parameters = new
            {
                Username = username
            };
            var result = await connection.QueryFirstOrDefaultAsync<AppUser>(
                SP_GET_BY_USERNAME, parameters, commandType: CommandType.StoredProcedure);
            return result;
        }
    }
}
