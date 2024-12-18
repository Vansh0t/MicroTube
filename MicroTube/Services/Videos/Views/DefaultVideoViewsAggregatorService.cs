﻿using EntityFramework.Exceptions.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MicroTube.Data.Access;
using MicroTube.Data.Models.Videos;
using MicroTube.Services.Search;
using System.Net;

namespace MicroTube.Services.VideoContent.Views
{
    public class DefaultVideoViewsAggregatorService : IVideoViewsAggregatorService
	{
		private readonly ILogger<DefaultVideoViewsAggregatorService> _logger;
		private readonly MicroTubeDbContext _db;

		public DefaultVideoViewsAggregatorService(
			ILogger<DefaultVideoViewsAggregatorService> logger,
			MicroTubeDbContext db)
		{
			_logger = logger;
			_db = db;
		}
		public async Task Aggregate()
		{
			using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead);
			try
			{
				VideoView[] views = await _db.VideoViews.Include(_ => _.Video).ToArrayAsync();
				IEnumerable<Guid> uniqueVideoIds = views.DistinctBy(_ => _.VideoId).Select(_ => _.VideoId);
				VideoViewsAggregation[] viewsAggregations = await _db.VideoAggregatedViews.Where(_=>uniqueVideoIds.Contains(_.VideoId)).ToArrayAsync();
				_logger.LogInformation($"Aggregating video views. Views to aggregate: {views.Count()}, videos: {viewsAggregations.Length}");
				foreach (var viewsAggregation in viewsAggregations)
				{
					int newViews = views.Count(_ => _.VideoId == viewsAggregation.VideoId);
					viewsAggregation.Views += newViews;
				}
				_db.RemoveRange(views);
				VideoSearchIndexing[] videoIndexing = await _db.VideoSearchIndexing.Where(_ => uniqueVideoIds.Contains(_.VideoId)).ToArrayAsync();
                foreach (var indexing in videoIndexing)
                {
					indexing.ReindexingRequired = true;
                }
                await _db.SaveChangesAsync();
				await transaction.CommitAsync();
				_logger.LogInformation($"Video views aggregation finished.");
			}
			catch(Exception e)
			{
				await transaction.RollbackAsync();
				_logger.LogError(e, "Failed to aggregate video views");
			}
			
		}
		public async Task<IServiceResult> CreateViewForAggregation(string videoId, string ip)
		{
			if (!IPAddress.TryParse(ip, out var ipAddress))
			{
				return ServiceResult.Fail(400, "Invalid source address");
			}
			if (!Guid.TryParse(videoId, out var guidVideoId))
			{
				return ServiceResult.Fail(400, "Invalid video id");
			}
			try
			{
				VideoView view = new VideoView { VideoId = guidVideoId, Ip = ip };
				_db.Add(view);
				await _db.SaveChangesAsync();
				return new ServiceResult(202);
			}
			catch (ReferenceConstraintException)
			{
				return ServiceResult.Fail(404, "Video was not found");
			}
			catch (UniqueConstraintException)
			{
				return new ServiceResult(202); //there is already a like from the same ip, so success anyway
			}
			catch (Exception e)
			{
				_logger.LogError(e, $"Failed to add view for aggregation to video {videoId}");
				return ServiceResult.FailInternal();
			}
		}
	}
}
