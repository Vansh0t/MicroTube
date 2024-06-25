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
				options.Description,
				options.LengthSeconds,
				options.Format,
				options.Fps,
				options.FrameSize,
				options.Timestamp
			};
			string sql = @"INSERT INTO dbo.VideoUploadProgress(
								RemoteCacheFileName, 
								RemoteCacheLocation, 
								UploaderId, 
								Title, 
								Description, 
								LengthSeconds,
								Fps,
								Format,
								FrameSize,
								Timestamp)
							OUTPUT INSERTED.*
							VALUES(
								@RemoteCacheFileName, 
								@RemoteCacheLocation, 
								@UploaderId, 
								@Title, 
								@Description, 
								@LengthSeconds,
								@Fps,
								@Format,
								@FrameSize,
								@Timestamp);";
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
		public async Task<int> UpdateUploadProgress(VideoUploadProgress uploadProgress)
		{
			//var parameters = new
			//{
			//	uploadProgress.Id,
			//	uploadProgress.Title,
			//	uploadProgress.Description,
			//	uploadProgress.UploaderId,
			//	uploadProgress.Status,
			//	uploadProgress.Message,
			//	uploadProgress.
			//};
			string sql = @"UPDATE dbo.VideoUploadProgress 
							SET 
								Title = @Title, 
								Description = @Description, 
								UploaderId = @UploaderId,
								Status = @Status,
								Message = @Message,
								LengthSeconds = @LengthSeconds,
								Fps = @Fps,
								Format = @Format,
								FrameSize = @FrameSize,
								Timestamp = @Timestamp
							WHERE Id = @Id;";
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			var result = await connection.ExecuteAsync(sql, uploadProgress);
			return result;
		}
		public async Task<VideoUploadProgress?> GetUploadProgressByFileName(string fileName)
		{
			var parameters = new
			{
				RemoteCacheFileName = fileName
			};
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			string sql = @"SELECT * FROM dbo.VideoUploadProgress WHERE RemoteCacheFileName = @RemoteCacheFileName;";
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
		public async Task<Video?> CreateVideo(Video video)
		{
			var parameters = new
			{
				video.UploaderId,
				video.Title,
				video.Description,
				video.Url,
				video.SnapshotUrls,
				video.ThumbnailUrls,
				video.UploadTime,
				video.LengthSeconds,
				video.SearchIndexId
			};
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			string sql = @"INSERT INTO dbo.Video(UploaderId, Title, Description, Url, SnapshotUrls, ThumbnailUrls, UploadTime, LengthSeconds, SearchIndexId)
							OUTPUT INSERTED.*
							VALUES(@UploaderId, @Title, @Description, @Url, @SnapshotUrls, @ThumbnailUrls, @UploadTime, @LengthSeconds, @SearchIndexId);";
			var result = await connection.QueryFirstOrDefaultAsync<Video>(sql, parameters);
			return result;
		}
		public async Task UpdateVideo(Video video)
		{
			var parameters = new
			{
				video.Id,
				video.UploaderId,
				video.Title,
				video.Description,
				video.Url,
				video.SnapshotUrls,
				video.ThumbnailUrls,
				video.UploadTime,
				video.LengthSeconds,
				video.SearchIndexId
			};
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			string sql = @"UPDATE dbo.Video 
						   SET 
						   UploaderId = @UploaderId,
						   Title = @Title,
						   Description = @Description,
						   Url = @Url,
						   SnapshotUrls = @SnapshotUrls,
						   ThumbnailUrls = @ThumbnailUrls,
						   UploadTime = @UploadTime,
						   LengthSeconds = @LengthSeconds,
						   SearchIndexId = @SearchIndexId
						   WHERE Id = @Id;";
			await connection.ExecuteAsync(sql, parameters);
		}
		//TODO: add filtering, sorting, etc
		public async Task<IEnumerable<Video>> GetVideos()
		{
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			string sql = @"SELECT * FROM dbo.Video;";
			var result = await connection.QueryAsync<Video>(sql);
			return result;
		}
		public async Task<Video?> GetVideo(string id)
		{
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			var parameters = new
			{
				Id = id
			};
			string sql = @"SELECT * FROM dbo.Video WHERE Id = @Id;";
			var result = await connection.QueryFirstOrDefaultAsync<Video>(sql, parameters);
			return result;
		}
	}
}
