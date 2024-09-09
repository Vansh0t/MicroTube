using Microsoft.EntityFrameworkCore;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using System.Data;

namespace MicroTube.Services.Search
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
			var indexRequired = await _db.VideoSearchIndexing
				.Include(_ => _.Video)
					.ThenInclude(_=>_!.VideoViews)
				.Include(_=>_.Video)
					.ThenInclude(_=>_!.VideoReactions)
				.Where(_ => _.ReindexingRequired)
				.ToArrayAsync();
			var indexRequiredVideos = indexRequired.Where(_ => _.Video != null).Select(_ => _.Video);
			var indexTasks = indexRequiredVideos.Select(IndexVideoNonThrowing!);
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
			_logger.LogInformation($"Ensuring search indices for videos done. Success: {successfulTasks}, fail: {failedTasks}");
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
	}

}
