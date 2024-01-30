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
		private const string SP_GET_SESSION_BY_ID = "dbo.AppUserSession_GetById";
		private const string SP_CREATE_USED_REFRESH_TOKEN = "dbo.UsedRefreshToken_Create";

		private readonly IConfiguration _configuration;

		public AppUserSessionDataAccess(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task CreateSession(string userId, string token, DateTime issuedDateTime, DateTime expiresDateTime)
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

		public async Task<AppUserSession?> GetSessionById(string sessionId)
		{
			var parameters = new
			{
				Id = sessionId
			};
			using IDbConnection connection = new SqlConnection(_configuration.GetDefaultConnectionString());
			var result = await connection.QueryFirstOrDefaultAsync<AppUserSession>(SP_GET_SESSION_BY_ID, parameters, commandType:CommandType.StoredProcedure);
			return result;
		}

		public async Task<AppUserSession?> GetSessionByToken(string token)
		{
			using IDbConnection connection = new SqlConnection(_configuration.GetDefaultConnectionString());
			var parameters = new
			{
				Token = token
			};
			AppUserSession? session = null;
			var result = await connection.QueryAsync<UsedRefreshToken, AppUserSession, UsedRefreshToken>(SP_GET_SESSION_BY_TOKEN, (usedToken, userSession) =>
			{
				if (session == null)
					session = userSession;
				usedToken.Session = session;
				session.UsedTokens.Add(usedToken);
				return usedToken;
			},
			parameters, commandType:CommandType.StoredProcedure);
			return session;
		}
		public async Task<UsedRefreshToken?> GetUsedRefreshToken(string token)
		{
			string sql = @"SELECT * FROM dbo.UsedRefreshToken WHERE Token = @Token";
			var parameters = new
			{
				Token = token
			};
			using IDbConnection connection = new SqlConnection(_configuration.GetDefaultConnectionString());
			var result = await connection.QueryFirstOrDefaultAsync<UsedRefreshToken>(sql, parameters);
			return result;
		}
		public async Task UpdateSession(AppUserSession session, IEnumerable<UsedRefreshToken>? newUsedRefreshTokens)
		{
			using IDbConnection connection = new SqlConnection(_configuration.GetDefaultConnectionString());
			var parameters = new
			{
				session.Id,
				session.Token,
				session.IssuedDateTime,
				session.ExpirationDateTime,
				session.IsInvalidated
			};
			connection.Open();
			using var transaction = connection.BeginTransaction();
			try
			{
				await connection.ExecuteAsync(SP_UPDATE_SESSION, parameters, transaction, commandType: CommandType.StoredProcedure);
				if(newUsedRefreshTokens != null)
				{
					foreach (var newUsedToken in newUsedRefreshTokens)
					{
						var usedRefreshTokenParameters = new { newUsedToken.SessionId, newUsedToken.Token };
						await connection.ExecuteAsync(SP_CREATE_USED_REFRESH_TOKEN, usedRefreshTokenParameters, transaction, commandType: CommandType.StoredProcedure);
					}
				}
				transaction.Commit();
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
		}
	}
}
