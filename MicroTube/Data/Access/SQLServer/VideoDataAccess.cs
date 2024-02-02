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
		public async Task<VideoUploadProgress?> CreateUploadProgress(string localFullPath, string uploaderId, string title, string? description)
		{
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			var parameters = new
			{
				LocalFullPath = localFullPath,
				UploaderId = uploaderId,
				Title = title,
				Description = description
			};
			string sql = @"INSERT INTO dbo.VideoUploadProgress(LocalFullPath, UploaderId, Title, Description)
							OUTPUT INSERTED.*
							VALUES(@LocalFullPath, @UploaderId, @Title, @Description);";
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
		public async Task<int> UpdateUploadStatus(string id, VideoUploadStatus status)
		{
			var parameters = new
			{
				Id = id,
				Status = status
			};
			string sql = @"UPDATE dbo.VideoUploadProgress 
							SET Status = @Status
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
