using Microsoft.EntityFrameworkCore;
using MicroTube.Data.Access;
using MicroTube.Data.Models.Comments;
using MicroTube.Services.ConfigOptions;

namespace MicroTube.Services.Search.Comments
{
	public class VideoCommentSearchService : IVideoCommentSearchService
	{
		private readonly MicroTubeDbContext _db;
		private readonly ISearchMetaProvider<IEnumerable<VideoComment>, VideoCommentSearchMeta> _metaProvider;
		private readonly IConfiguration _config;
		private readonly ILogger<VideoCommentSearchService> _logger;

		public VideoCommentSearchService(
			MicroTubeDbContext db,
			ISearchMetaProvider<IEnumerable<VideoComment>, VideoCommentSearchMeta> metaProvider,
			IConfiguration config,
			ILogger<VideoCommentSearchService> logger)
		{
			_db = db;
			_metaProvider = metaProvider;
			_config = config;
			_logger = logger;
		}

		public async Task<IServiceResult<VideoCommentSearchResult>> GetComments(string videoId, VideoCommentSearchParameters parameters, string? meta)
		{
			var options = _config.GetRequiredByKey<VideoCommentSearchOptions>(VideoCommentSearchOptions.KEY);
			int batchSize = Math.Min(options.PaginationMaxBatchSize, parameters.BatchSize);
			try
			{
				VideoCommentSearchMeta? parsedMeta = _metaProvider.DeserializeMeta(meta);
				IEnumerable<VideoComment> result = new VideoComment[0];
				switch (parameters.SortType)
				{
					case VideoCommentSortType.Top:
						result = await GetTopCommentsBatch(batchSize, parsedMeta);
						break;
					case VideoCommentSortType.Newest:
						result = await GetLatestCommentsBatch(batchSize, parsedMeta);
						break;
				}
				var newMeta = result != null ? _metaProvider.BuildMeta(result) : null;
				var serializedMeta = newMeta != null ? _metaProvider.SerializeMeta(newMeta) : null;
				return ServiceResult<VideoCommentSearchResult>.Success(new VideoCommentSearchResult { Comments = result, Meta = serializedMeta });
			}
			catch (Exception e)
			{
				_logger.LogError(e, $"Failed to get comments for video: {videoId}. Sort: {parameters.SortType}, BatchSize: {parameters.BatchSize}, Meta: {meta}");
				return ServiceResult<VideoCommentSearchResult>.FailInternal();
			}

		}
		private async Task<IEnumerable<VideoComment>> GetTopCommentsBatch(int batchSize, VideoCommentSearchMeta? meta)
		{
			int lastRank = meta != null ? meta.LastLikes - meta.LastDislikes : int.MaxValue;
			Guid lastGuid = meta != null ? new Guid(meta.LastId) : Guid.Empty;
			DateTime lastTime = meta != null ? meta.LastTime : DateTime.MaxValue;
			var result = await _db.VideoComments
				.Include(_ => _.Reactions)
				.Include(_=>_.User)
				.Where(_ => !_.Deleted && _.Id != lastGuid && (_.Reactions!.Difference < lastRank || (_.Reactions!.Difference == lastRank && _.Time < lastTime)))
				.OrderByDescending(_ => _.Reactions!.Difference)
				.ThenByDescending(_=>_.Time)
				.Take(batchSize)
				.ToArrayAsync();
			return result;
		}
		private async Task<IEnumerable<VideoComment>> GetLatestCommentsBatch(int batchSize, VideoCommentSearchMeta? meta)
		{
			DateTime lastTime = meta != null ? meta.LastTime : DateTime.MaxValue;
			Guid lastGuid = meta != null ? new Guid(meta.LastId) : Guid.Empty;
			var result = await _db.VideoComments
				.Include(_ => _.Reactions)
				.Include(_=>_.User)
				.Where(_ => !_.Deleted && _.Id != lastGuid && _.Time < lastTime)
				.OrderByDescending(_ => _.Time)
				.Take(batchSize)
				.ToArrayAsync();
			return result;
		}
	}
}