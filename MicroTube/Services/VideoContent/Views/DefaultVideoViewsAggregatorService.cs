using Microsoft.Data.SqlClient;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.Search;

namespace MicroTube.Services.VideoContent.Views
{
	public class DefaultVideoViewsAggregatorService : IVideoViewsAggregatorService
	{
		private readonly ILogger<DefaultVideoViewsAggregatorService> _logger;
		private readonly IConfiguration _config;
		private readonly IVideoDataAccess _videoDataAccess;
		private readonly IVideoSearchService _videoSearch;

		public DefaultVideoViewsAggregatorService(
			ILogger<DefaultVideoViewsAggregatorService> logger,
			IConfiguration config,
			IVideoDataAccess videoDataAccess,
			IVideoSearchService videoSearch)
		{
			_logger = logger;
			_config = config;
			_videoDataAccess = videoDataAccess;
			_videoSearch = videoSearch;
		}
		public async Task Aggregate()
		{
			IEnumerable<VideoView> views = await _videoDataAccess.GetVideoViews();
			Video[] uniqueVideos = views.Select(_ => _.Video).DistinctBy(_ => _.Id).ToArray();
			_logger.LogInformation($"Aggregating video views. Views to aggregate: {views.Count()}, videos: {uniqueVideos.Length}");
			await _videoDataAccess.DeleteVideoViews(views.Select(_ => _.Id.ToString()));
			foreach (var video in uniqueVideos)
			{
				int newViews = views.Count(_ => _.VideoId == video.Id);
				video.Views += newViews;
			}
			var results = await Task.WhenAll(uniqueVideos.Select(UpdateVideoNonThrowing));
			int successfulTasks = 0;
			int failedTasks = 0;
			foreach (var result in results)
			{
				if (result.IsError)
				{
					_logger.LogError("A video views update failed, " + result.Error);
					failedTasks++;
					continue;
				}
				successfulTasks++;
			}
			_logger.LogInformation($"Video views aggregation finished. Success: {successfulTasks}, fail: {failedTasks}");
			await Task.WhenAll(uniqueVideos.Select(_videoSearch.IndexVideo));//move this into video indexing
		}
		public async Task<IServiceResult> CreateViewForAggregation(string videoId, string ip)
		{
			try
			{
				await _videoDataAccess.AddVideoView(videoId, ip);
				return new ServiceResult(202);
			}
			catch (SqlException e)
			{
				//unique key constraint violation
				if (e.Number == 2627)
				{
					return new ServiceResult(202); //there is already a like from the same ip, so success anyway
				}
				_logger.LogError(e, $"Failed to add view for aggregation to video {videoId}");
				return ServiceResult<VideoLike>.FailInternal();
			}
			catch (Exception e)
			{
				_logger.LogError(e, $"Failed to add view for aggregation to video {videoId}");
				return ServiceResult<VideoLike>.FailInternal();
			}
		}
		private async Task<IServiceResult> UpdateVideoNonThrowing(Video video)
		{
			try
			{
				await _videoDataAccess.UpdateVideo(video);
				return ServiceResult.Success();
			}
			catch (Exception e)
			{
				return ServiceResult.Fail(500, $"Failed to update video {video.Id} due to unhandled exception: " + e.ToString());
			}
		}
	}
}
