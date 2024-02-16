using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MicroTube.Data.Models;
using System.Data;

namespace MicroTube.Data.Access.SQLServer
{
    public class VideoDataAccess : IVideoDataAccess
	{
		private readonly IConfiguration _config;

		public VideoDataAccess(IConfiguration config)
		{
			_config = config;
		}
		public async Task<VideoUploadProgress?> CreateUploadProgress(VideoUploadProgressCreationOptions options)
		{
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			var parameters = new
			{
				options.RemoteCacheFileName,
				options.RemoteCacheLocation,
				options.UploaderId,
				options.Title,
				options.Description
			};
			string sql = @"INSERT INTO dbo.VideoUploadProgress(RemoteCacheFileName, RemoteCacheLocation, UploaderId, Title, Description)
							OUTPUT INSERTED.*
							VALUES(@RemoteCacheFileName, @RemoteCacheLocation, @UploaderId, @Title, @Description);";
			var result = await connection.QueryFirstOrDefaultAsync<VideoUploadProgress>(sql, parameters);
			return result;
		}
		public async Task<VideoUploadProgress?> GetUploadProgressById(string id)
		{
			var parameters = new
			{
				Id = id
			};
			string sql = @"SELECT * FROM dbo.VideoUploadProgress WHERE Id = @Id;";
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			var result = await connection.QueryFirstOrDefaultAsync<VideoUploadProgress>(sql, parameters);
			return result;
		}
		public async Task<int> UpdateUploadProgress(string id, VideoUploadStatus status, string? message)
		{
			var parameters = new
			{
				Id = id,
				Status = status,
				Message = message
			};
			string sql = @"UPDATE dbo.VideoUploadProgress 
							SET Status = @Status, Message = @Message
							WHERE Id = @Id;";
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			var result = await connection.ExecuteAsync(sql, parameters);
			return result;
		}
		public async Task<VideoUploadProgress?> GetUploadProgressByLocalFullPath(string localFullPath)
		{
			var parameters = new
			{
				LocalFullPath = localFullPath
			};
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			string sql = @"SELECT * FROM dbo.VideoUploadProgress WHERE LocalFullPath = @LocalFullPath;";
			var result = await connection.QueryFirstOrDefaultAsync<VideoUploadProgress>(sql, parameters);
			return result;
		} 
		public async Task<IEnumerable<VideoUploadProgress>> GetVideoUploadProgressListForUser(string userId)
		{
			var parameters = new
			{
				UploaderId = userId
			};
			string sql = @"SELECT * FROM dbo.VideoUploadProgress WHERE UploaderId = @UploaderId";
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			var result = await connection.QueryAsync<VideoUploadProgress>(sql, parameters);
			return result;
		}
	}
}
