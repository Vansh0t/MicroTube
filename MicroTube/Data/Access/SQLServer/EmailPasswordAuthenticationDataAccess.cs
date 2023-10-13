using Dapper;
using Microsoft.Data.SqlClient;
using MicroTube.Data.Models;
using System.Data;

namespace MicroTube.Data.Access.SQLServer
{
    public class EmailPasswordAuthenticationDataAccess : IAuthenticationDataAccess<EmailPasswordAuthentication>
    {
        private const string SP_CREATE_USER = "dbo.AppUser_CreatePasswordEmail";
        private const string SP_GET = "dbo.EmailPasswordAuthentication_Get";
        private const string SP_UPDATE_EMAIL = "dbo.EmailPasswordAuthentication_UpdateEmail";
        private const string SP_UPDATE_EMAIL_CONFIRMATION = "dbo.EmailPasswordAuthentication_UpdateEmailConfirmation";
        private const string SP_UPDATE_PASSWORD = "dbo.EmailPasswordAuthentication_UpdatePassword";
        private const string SP_UPDATE_PASSWORD_RESET = "dbo.EmailPasswordAuthentication_UpdatePasswordReset";


        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailPasswordAuthenticationDataAccess> _logger;

        //private IDbConnection? atomicConnection;
        //private IDbTransaction? atomicTransaction;
        public EmailPasswordAuthenticationDataAccess(IConfiguration configuration, ILogger<EmailPasswordAuthenticationDataAccess> logger)
        {
            _logger = logger;
            _configuration = configuration;
        }

        

        public async Task<int> CreateUser(string username, string email, EmailPasswordAuthentication auth)
        {
            using IDbConnection connection = new SqlConnection(_configuration.GetDefaultConnectionString());
            var parameters = new DynamicParameters();
            parameters.Add("Username", username);
            parameters.Add("Email", email);
            parameters.Add("PasswordHash", auth.PasswordHash);
            parameters.Add("EmailConfirmationString", auth.EmailConfirmationString);
            parameters.Add("EmailConfirmationStringExpiration", auth.EmailConfirmationStringExpiration);
            parameters.Add("CreatedUserId", 0, DbType.Int32, ParameterDirection.Output);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync(SP_CREATE_USER, parameters, transaction, commandType: CommandType.StoredProcedure);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return parameters.Get<int>("@CreatedUserId");
        }

        
        public async Task<EmailPasswordAuthentication?> Get(int userId)
        {
            using IDbConnection connection = new SqlConnection(_configuration.GetDefaultConnectionString());
            var parameters = new
            {
                UserId = userId
            };
            var result = await connection.QueryAsync<EmailPasswordAuthentication>(
                SP_GET, parameters, commandType: CommandType.StoredProcedure);
            return result.FirstOrDefault();
        }

        public async Task UpdateEmailConfirmation(EmailPasswordAuthentication auth)
        {
            using IDbConnection connection = new SqlConnection(_configuration.GetDefaultConnectionString());
            var parameters = new
            {
                auth.UserId,
                auth.EmailConfirmationString,
                auth.EmailConfirmationStringExpiration
            };
            await connection.ExecuteAsync(
                SP_UPDATE_EMAIL_CONFIRMATION, parameters, commandType: CommandType.StoredProcedure);
        }
        public async Task UpdatePasswordReset(EmailPasswordAuthentication auth)
        {
            using IDbConnection connection = new SqlConnection(_configuration.GetDefaultConnectionString());
            var parameters = new
            {
                auth.UserId,
                auth.PasswordResetString,
                auth.PasswordResetStringExpiration
            };
            await connection.ExecuteAsync(
                SP_UPDATE_PASSWORD_RESET, parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task UpdateEmailAndConfirmation(EmailPasswordAuthentication auth, string newEmail)
        {
            using IDbConnection connection = new SqlConnection(_configuration.GetDefaultConnectionString());
            var confirmationUpdateParameters = new
            {
                auth.UserId,
                auth.EmailConfirmationString,
                auth.EmailConfirmationStringExpiration
            };
            var emailUpdateParameters = new
            {
                auth.UserId,
                Email = newEmail,
            };
            connection.Open();
            using IDbTransaction transaction = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync(
                    SP_UPDATE_EMAIL_CONFIRMATION, confirmationUpdateParameters, transaction, commandType: CommandType.StoredProcedure);
                await connection.ExecuteAsync(
                    SP_UPDATE_EMAIL, emailUpdateParameters, transaction, commandType: CommandType.StoredProcedure);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            
        }
        public async Task UpdatePasswordHashAndReset(EmailPasswordAuthentication auth, string passwordHash)
        {
            using IDbConnection connection = new SqlConnection(_configuration.GetDefaultConnectionString());
            var resetUpdateParameters = new
            {
                auth.UserId,
                auth.PasswordResetString,
                auth.PasswordResetStringExpiration
            };
            var passwordUpdateParameters = new
            {
                auth.UserId,
                PasswordHash = passwordHash,
            };
            connection.Open();
            using IDbTransaction transaction = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync(
                    SP_UPDATE_PASSWORD_RESET, resetUpdateParameters, transaction, commandType: CommandType.StoredProcedure);
                await connection.ExecuteAsync(
                    SP_UPDATE_PASSWORD, passwordUpdateParameters, transaction, commandType: CommandType.StoredProcedure);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        //public void EndAtomic(bool commit)
        //{
        //    if(atomicConnection == null || atomicTransaction == null)
        //    {
        //        throw new DataAccessException("Tried to end non existing atomic transaction");
        //    }
        //    if(commit)
        //    {
        //        atomicTransaction.Commit();
        //    }
        //    else
        //    {
        //        atomicTransaction.Rollback();
        //    }
        //    atomicConnection.Dispose();
        //    atomicConnection = null;
        //    atomicTransaction = null;
        //}
        //public void BeginAtomic()
        //{
        //    if (atomicConnection != null || atomicTransaction != null)
        //        throw new DataAccessException("Tried to begin new atomic transaction while previous wasn't ended.");
        //    atomicConnection = new SqlConnection(_configuration.GetDefaultConnectionString());
        //    atomicConnection.Open();
        //    atomicTransaction = atomicConnection.BeginTransaction();
        //}
    }
}
