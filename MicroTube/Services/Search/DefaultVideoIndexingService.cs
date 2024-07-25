using Dapper;
using Microsoft.Data.SqlClient;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using System.Data;

namespace MicroTube.Services.Search
{
	public class DefaultVideoIndexingService : IVideoIndexingService
	{
		private readonly IConfiguration _config;
		private readonly ILogger<DefaultVideoIndexingService> _logger;
		private readonly IVideoDataAccess _videoAccess;
		private readonly IVideoSearchService _videoSearch;

		public DefaultVideoIndexingService(IConfiguration config, ILogger<DefaultVideoIndexingService> logger, IVideoDataAccess videoAccess, IVideoSearchService videoSearch)
		{
			_config = config;
			_logger = logger;
			_videoAccess = videoAccess;
			_videoSearch = videoSearch;
		}

		public async Task EnsureVideoIndices()
		{
			_logger.LogInformation("Ensuring search indices for videos.");
			var unindexedVideos = await GetUnindexedVideos();
			var indexTasks = unindexedVideos.Select(_ => IndexVideoNonThrowing(_));
			await Task.WhenAll(indexTasks);
			List<Task<IServiceResult>> videosUpdateTasks = new List<Task<IServiceResult>>();
			int failedTasks = 0;
			foreach (var indexTask in indexTasks)
			{
				var result = indexTask.Result;
				if (result.IsError)
				{
					failedTasks++;
					_logger.LogError("Failed to index a video for search. " + result.Error);
					continue;
				}
				var updatedVideo = result.GetRequiredObject();
				var saveUpdatedVideoTask = UpdateVideoNonThrowing(updatedVideo);
				videosUpdateTasks.Add(saveUpdatedVideoTask);
			}
			await Task.WhenAll(videosUpdateTasks);
			int successfulTasks = 0; 
			foreach(var videoUpdateTask in videosUpdateTasks)
			{
				var result = videoUpdateTask.Result;
				if(result.IsError)
				{
					_logger.LogError("A video was indexed, but database update failed. " + result.Error);
					failedTasks++;
					continue;
				}
				successfulTasks++;
			}
			_logger.LogInformation($"Ensuring search indices for videos done. Success: {successfulTasks}, fail: {failedTasks}");
		}
		private async Task<IEnumerable<Video>> GetUnindexedVideos()
		{
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			string sql = @"SELECT * FROM dbo.Video WHERE SearchIndexId IS NULL OR SearchIndexId = '';";
			var result = await connection.QueryAsync<Video>(sql);
			return result;
		}
		private async Task<IServiceResult<Video>> IndexVideoNonThrowing(Video video)
		{
			try
			{
				var result  = await _videoSearch.IndexVideo(video);
				return result;
			}
			catch(Exception e)
			{
				return ServiceResult<Video>.Fail(500, $"Failed to index video {video.Id} due to unhandled exception: " + e.ToString());
			}
		}
		private async Task<IServiceResult> UpdateVideoNonThrowing(Video video)
		{
			try
			{
				await _videoAccess.UpdateVideo(video);
				return ServiceResult.Success();
			}
			catch (Exception e)
			{
				return ServiceResult<Video>.Fail(500, $"Failed update indexed video {video.Id} due to unhandled exception: " + e.ToString());
			}
		}
	}

}
