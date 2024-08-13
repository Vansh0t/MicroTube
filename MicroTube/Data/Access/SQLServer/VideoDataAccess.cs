using Dapper;
using Elastic.Clients.Elasticsearch;
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
				video.Urls,
				video.SnapshotUrls,
				video.ThumbnailUrls,
				video.UploadTime,
				video.LengthSeconds,
				video.SearchIndexId,
				video.Views,
				video.Likes
			};
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			string sql = @"INSERT INTO dbo.Video(UploaderId, Title, Description, Urls, SnapshotUrls, ThumbnailUrls, UploadTime, LengthSeconds, SearchIndexId, Views, Likes)
							OUTPUT INSERTED.*
							VALUES(@UploaderId, @Title, @Description, @Urls, @SnapshotUrls, @ThumbnailUrls, @UploadTime, @LengthSeconds, @SearchIndexId, @Views, @Likes);";
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
				video.Urls,
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
						   Urls = @Urls,
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

		public async Task<IEnumerable<Video>> GetVideosByIds(IEnumerable<string> ids)
		{
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			var parameters = new
			{
				Ids = ids
			};
			string sql = @"SELECT * FROM dbo.Video WHERE Id IN @Ids;";
			var result = await connection.QueryAsync<Video>(sql, parameters);
			return result;
		}

		public async Task<VideoLike?> GetLike(string userId, string videoId)
		{
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			var parameters = new
			{
				UserId = userId,
				VideoId = videoId
			};
			string sql = @"SELECT TOP 1 [like].*, [user].*, [video].*
						   FROM dbo.VideoLike [like]
						   INNER JOIN dbo.AppUser [user] ON [like].UserId = [user].Id
						   INNER JOIN dbo.Video video ON [like].VideoId = video.Id
						   WHERE UserId = @UserId AND VideoId = @VideoId;";
			var result = await connection.QueryAsync<VideoLike, AppUser, Video, VideoLike>(sql, (like, user, video) =>
			{
				like.Video = video;
				like.User = user;
				return like;
			}, parameters);
			return result.FirstOrDefault();
		}

		public async Task<VideoDislike?> GetDislike(string userId, string videoId)
		{
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			var parameters = new
			{
				UserId = userId,
				VideoId = videoId
			};
			string sql = @"SELECT TOP 1 [dislike].*, [user].*, [video].*
						   FROM dbo.VideoDislike [dislike]
						   INNER JOIN dbo.AppUser [user] ON [dislike].UserId = [user].Id
						   INNER JOIN dbo.Video video ON [dislike].VideoId = video.Id
						   WHERE UserId = @UserId AND VideoId = @VideoId;";
			var result = await connection.QueryAsync<VideoDislike, AppUser, Video, VideoDislike>(sql, (dislike, user, video) =>
			{
				dislike.Video = video;
				dislike.User = user;
				return dislike;
			}, parameters);
			return result.FirstOrDefault();
		}
	}
}
