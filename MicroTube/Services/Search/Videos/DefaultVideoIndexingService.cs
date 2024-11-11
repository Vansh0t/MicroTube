using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using MicroTube.Data.Access;
using MicroTube.Data.Models.Videos;
using System.Data;

namespace MicroTube.Services.Search.Videos
{
    public class DefaultVideoIndexingService : IVideoIndexingService
	{
		private readonly ILogger<DefaultVideoIndexingService> _logger;
		private readonly IVideoSearchService _videoSearch;
		private readonly MicroTubeDbContext _db;
		public DefaultVideoIndexingService(ILogger<DefaultVideoIndexingService> logger, IVideoSearchService videoSearch, MicroTubeDbContext db)
		{
			_logger = logger;
			_videoSearch = videoSearch;
			_db = db;
		}

		public async Task EnsureVideoIndices()
		{
			_logger.LogInformation("Ensuring search indices for videos.");
			var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
			var videoToReindex = await _db.Videos
				.Include(_ => _.VideoIndexing)
				.Include(_ => _.VideoViewsAggregation)
				.Include(_ => _.VideoReactionsAggregation)
				.Where(_ => _.VideoIndexing!.ReindexingRequired)
				.ToArrayAsync();
			var indexTasks = videoToReindex.Select(IndexVideoNonThrowing!);
			await Task.WhenAll(indexTasks);
			int failedTasks = 0;
			int successfulTasks = 0;
			foreach (var indexTask in indexTasks)
			{
				var result = indexTask.Result;
				if (result.IsError)
				{
					failedTasks++;
					_logger.LogError("Failed to index a video for search. " + result.Error);
				}
				else
					successfulTasks++;
			}
			await _db.SaveChangesAsync();
			await transaction.CommitAsync();
			_logger.LogInformation($"Ensuring search indices for videos done. Success: {successfulTasks}, fail: {failedTasks}");
		}
		private async Task<IServiceResult<Video>> IndexVideoNonThrowing(Video video)
		{
			try
			{
				Guard.Against.Null(video.VideoIndexing);
				var result  = await _videoSearch.IndexVideo(video);
				return result;
			}
			catch(Exception e)
			{
				return ServiceResult<Video>.Fail(500, $"Failed to index video {video.Id} due to unhandled exception: " + e.ToString());
			}
		}
	}

}
