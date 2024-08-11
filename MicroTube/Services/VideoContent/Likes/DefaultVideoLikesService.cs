using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using Elastic.Clients.Elasticsearch;

namespace MicroTube.Services.VideoContent.Likes
{
	public class DefaultVideoLikesService : IVideoLikesService
	{
		private readonly IConfiguration _config;
		private readonly ILogger<DefaultVideoLikesService> _logger;
		private readonly IVideoSearchDataAccess _searchDataAccess;
		private readonly IVideoDataAccess _videoDataAccess;
		public DefaultVideoLikesService(
			IConfiguration config,
			ILogger<DefaultVideoLikesService> logger,
			IVideoSearchDataAccess searchDataAccess,
			IVideoDataAccess videoDataAccess)
		{
			_config = config;
			_logger = logger;
			_searchDataAccess = searchDataAccess;
			_videoDataAccess = videoDataAccess;
		}

		public async Task<IServiceResult<VideoLike>> LikeVideo(string userId, string videoId)
		{
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			connection.Open();
			using IDbTransaction transaction = connection.BeginTransaction();
			try
			{
				//TO DO: make sure video existance check not needed here
				await CreateLike(connection, transaction, userId, videoId);
				await IncrementVideoLikes(connection, transaction, videoId);
				var like = await GetVideoLike(connection, transaction, videoId, userId);
				if (like == null)
				{
					throw new DataAccessException($"Video like had been created but wasn't returned after request. User: {userId}, Video: {videoId}");
				}
				var videoIndex = await _searchDataAccess.GetVideoIndex(videoId);
				if (videoIndex != null)
				{
					videoIndex.Likes++;
					await _searchDataAccess.IndexVideo(videoIndex);
				}
				transaction.Commit();
				return ServiceResult<VideoLike>.Success(like);
			}
			catch (Exception e)
			{
				transaction.Rollback();
				_logger.LogError(e, $"Failed add video like from user {userId} to video {videoId}");
				return ServiceResult<VideoLike>.FailInternal();
			}
		}
		public async Task<IServiceResult<VideoLike>> GetLike(string userId, string videoId)
		{
			var like = await _videoDataAccess.GetLike(userId, videoId);
			if (like == null)
				return ServiceResult<VideoLike>.Fail(404, "Not found");
			return ServiceResult<VideoLike>.Success(like);
		}
		private async Task CreateLike(IDbConnection connection, IDbTransaction transaction, string userId, string videoId)
		{
			var parameters = new
			{
				UserId = userId,
				VideoId = videoId,
				Time = DateTime.UtcNow
			};
			string sql = "INSERT INTO dbo.VideoLike(UserId, VideoId, Time) VALUES(@UserId, @VideoId, @Time);";
			var result = await connection.ExecuteAsync(sql, parameters, transaction);
			_logger.LogInformation("Created Like " + result);
		}
		private async Task IncrementVideoLikes(IDbConnection connection, IDbTransaction transaction, string videoId)
		{
			var parameters = new
			{
				Id = videoId
			};
			string sql = "UPDATE dbo.Video SET Likes = Likes + 1 WHERE Id = @Id;";
			var result = await connection.ExecuteAsync(sql, parameters, transaction);
			_logger.LogInformation("Incremented Like " + result);
		}
		private async Task<VideoLike?> GetVideoLike(IDbConnection connection, IDbTransaction transaction, string videoId, string userId)
		{
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
			}, parameters, transaction);
			return result.FirstOrDefault();
		}
	}
}

