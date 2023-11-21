using Dapper;
using Microsoft.Data.SqlClient;
using MicroTube.Data.Models;
using System.Data;

namespace MicroTube.Data.Access.SQLServer
{
    public class AppUserSessionDataAccess : IUserSessionDataAccess
	{
		private const string SP_CREATE_SESSION = "dbo.AppUserSession_Create";
		private const string SP_UPDATE_SESSION = "dbo.AppUserSession_Update";
		private const string SP_GET_SESSION_BY_TOKEN = "dbo.AppUserSession_GetByToken";

		private readonly IConfiguration _configuration;

		public AppUserSessionDataAccess(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task CreateSession(int userId, string token, DateTime issuedDateTime, DateTime expiresDateTime)
		{
			using IDbConnection connection = new SqlConnection(_configuration.GetDefaultConnectionString());
			var parameters = new
			{
				UserId = userId,
				Token = token,
				IssuedDateTime= issuedDateTime,
				ExpirationDateTime = expiresDateTime
			};
			connection.Open();
			await connection.ExecuteAsync(SP_CREATE_SESSION, parameters, commandType: CommandType.StoredProcedure);
		}
		public async Task<AppUserSession?> GetSessionByToken(string token)
		{
			using IDbConnection connection = new SqlConnection(_configuration.GetDefaultConnectionString());
			var parameters = new
			{
				Token = token
			};
			connection.Open();
			return await connection.QueryFirstOrDefaultAsync<AppUserSession>(SP_GET_SESSION_BY_TOKEN, parameters, commandType: CommandType.StoredProcedure);
		}
		public async Task UpdateSession(AppUserSession session)
		{
			using IDbConnection connection = new SqlConnection(_configuration.GetDefaultConnectionString());
			var parameters = new
			{
				Id = session.Id,
				Token = session.Token,
				PreviousToken = session.PreviousToken,
				IssuedDateTime = session.IssuedDateTime,
				ExpirationDateTime = session.ExpirationDateTime,
				IsInvalidated = session.IsInvalidated
			};
			connection.Open();
			await connection.ExecuteAsync(SP_UPDATE_SESSION, parameters, commandType: CommandType.StoredProcedure);
		}
	}
}
