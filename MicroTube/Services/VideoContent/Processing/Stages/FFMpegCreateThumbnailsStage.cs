using Ardalis.GuardClauses;
using MicroTube.Services.ConfigOptions;

namespace MicroTube.Services.VideoContent.Processing.Stages
{
    public class FFMpegCreateThumbnailsStage : VideoProcessingStage
    {
        private readonly IVideoThumbnailsService _thumbnailService;
        private readonly IConfiguration _config;

		public FFMpegCreateThumbnailsStage(IVideoThumbnailsService thumbnailService, IConfiguration config)
		{
			_thumbnailService = thumbnailService;
			_config = config;
		}

		protected override async Task<DefaultVideoProcessingContext> ExecuteInternal(DefaultVideoProcessingContext? context, CancellationToken cancellationToken)
        {
			Guard.Against.Null(context);
			Guard.Against.Null(context.LocalCache);
			string thumbnailsSource = GetQualityTierSourceForThumbnailCreation(context);
			var thumbnailPaths = await MakeThumbnails(thumbnailsSource, context.LocalCache.ThumbnailsLocation, cancellationToken);
            return context;
        }
        private async Task<IEnumerable<string>> MakeThumbnails(string localCacheSourcePath, string saveToPath, CancellationToken cancellationToken)
        {
            var thumbnailsResult = await _thumbnailService.MakeThumbnails(localCacheSourcePath, saveToPath, cancellationToken);
            if (thumbnailsResult.IsError)
            {
                throw new BackgroundJobException($"Failed to create thumbnails. File: {localCacheSourcePath}, SaveTo: {saveToPath}. {thumbnailsResult.Error}");
            }
            return thumbnailsResult.GetRequiredObject();
        }
		private string GetQualityTierSourceForThumbnailCreation(DefaultVideoProcessingContext context)
		{
			Guard.Against.Null(context.LocalCache);
			Guard.Against.NullOrEmpty(context.LocalCache.QualityTierPaths);
			VideoProcessingOptions options = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
			return context.LocalCache.QualityTierPaths.Last(_ => _.Key <= options.ThumbnailsQualityTier).Value;
		}
    }
}
