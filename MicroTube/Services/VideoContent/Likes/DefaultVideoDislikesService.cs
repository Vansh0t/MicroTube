using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
namespace MicroTube.Services.VideoContent.Likes
{
	public class DefaultVideoDislikesService : IVideoDislikesService
	{
		private readonly IConfiguration _config;
		private readonly ILogger<DefaultVideoDislikesService> _logger;
		private readonly IVideoSearchDataAccess _searchDataAccess;
		private readonly IVideoDataAccess _videoDataAccess;
		public DefaultVideoDislikesService(
			IConfiguration config,
			ILogger<DefaultVideoDislikesService> logger,
			IVideoSearchDataAccess searchDataAccess,
			IVideoDataAccess videoDataAccess)
		{
			_config = config;
			_logger = logger;
			_searchDataAccess = searchDataAccess;
			_videoDataAccess = videoDataAccess;
		}

		public async Task<IServiceResult<VideoDislike>> DislikeVideo(string userId, string videoId)
		{
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			connection.Open();
			using IDbTransaction transaction = connection.BeginTransaction();
			try
			{
				//TO DO: make sure video existance check not needed here
				await CreateDislike(connection, transaction, userId, videoId);
				await IncrementVideoDislikes(connection, transaction, videoId);
				var dislike = await GetVideoDislike(connection, transaction, videoId, userId);
				if (dislike == null)
				{
					throw new DataAccessException($"Video like had been created but wasn't returned after request. User: {userId}, Video: {videoId}");
				}
				var videoIndex = await _searchDataAccess.GetVideoIndex(videoId);
				if (videoIndex != null)
				{
					videoIndex.Dislikes++;
					await _searchDataAccess.IndexVideo(videoIndex);
				}
				transaction.Commit();
				return ServiceResult<VideoDislike>.Success(dislike);
			}
			catch (SqlException e)
			{
				//unique key constraint violation
				transaction.Rollback();
				if (e.Number == 2627)
				{
					return ServiceResult<VideoDislike>.Fail(400, "Already disliked.");
				}
				_logger.LogError(e, $"Failed add video dislike from user {userId} to video {videoId}");
				return ServiceResult<VideoDislike>.FailInternal();
			}
			catch (Exception e)
			{
				transaction.Rollback();
				_logger.LogError(e, $"Failed add video dislike from user {userId} to video {videoId}");
				return ServiceResult<VideoDislike>.FailInternal();
			}
		}
		public async Task<IServiceResult> UndislikeVideo(string userId, string videoId)
		{
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			connection.Open();
			using IDbTransaction transaction = connection.BeginTransaction();
			try
			{
				var dislike = await GetVideoDislike(connection, transaction, videoId, userId);
				if (dislike != null)
				{
					await DecrementVideoDislikes(connection, transaction, videoId);
					await DeleteDislike(connection, transaction, dislike.Id.ToString());
					var videoIndex = await _searchDataAccess.GetVideoIndex(videoId);
					if (videoIndex != null)
					{
						videoIndex.Dislikes--;
						await _searchDataAccess.IndexVideo(videoIndex);
					}
				}
				transaction.Commit();
				return new ServiceResult(dislike != null ? 200 : 404);
			}
			catch (Exception e)
			{
				transaction.Rollback();
				_logger.LogError(e, $"Failed add video dislike from user {userId} to video {videoId}");
				return ServiceResult.FailInternal();
			}
		}
		public async Task<IServiceResult<VideoDislike>> GetDislike(string userId, string videoId)
		{
			var dislike = await _videoDataAccess.GetDislike(userId, videoId);
			if (dislike == null)
				return ServiceResult<VideoDislike>.Fail(404, "Not found");
			return ServiceResult<VideoDislike>.Success(dislike);
		}
		private async Task CreateDislike(IDbConnection connection, IDbTransaction transaction, string userId, string videoId)
		{
			var parameters = new
			{
				UserId = userId,
				VideoId = videoId,
				Time = DateTime.UtcNow
			};
			string sql = "INSERT INTO dbo.VideoDislike(UserId, VideoId, Time) VALUES(@UserId, @VideoId, @Time);";
			await connection.ExecuteAsync(sql, parameters, transaction);
		}
		private async Task DeleteDislike(IDbConnection connection, IDbTransaction transaction, string dislikeId)
		{
			var parameters = new
			{
				Id = dislikeId,
				Time = DateTime.UtcNow
			};
			string sql = "DELETE FROM dbo.VideoDislike WHERE Id = @Id;";
			await connection.ExecuteAsync(sql, parameters, transaction);
		}
		private async Task IncrementVideoDislikes(IDbConnection connection, IDbTransaction transaction, string videoId)
		{
			var parameters = new
			{
				Id = videoId
			};
			string sql = "UPDATE dbo.Video SET Dislikes = Dislikes + 1 WHERE Id = @Id;";
			await connection.ExecuteAsync(sql, parameters, transaction);
		}
		private async Task<VideoDislike?> GetVideoDislike(IDbConnection connection, IDbTransaction transaction, string videoId, string userId)
		{
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
			var result = await connection.QueryAsync<VideoDislike, AppUser, Video, VideoDislike>(sql, (like, user, video) =>
			{
				like.Video = video;
				like.User = user;
				return like;
			}, parameters, transaction);
			return result.FirstOrDefault();
		}
		private async Task DecrementVideoDislikes(IDbConnection connection, IDbTransaction transaction, string videoId)
		{
			var parameters = new
			{
				Id = videoId
			};
			string sql = "UPDATE dbo.Video SET Dislikes = Dislikes - 1 WHERE Id = @Id;";
			await connection.ExecuteAsync(sql, parameters, transaction);
		}
	}
}

