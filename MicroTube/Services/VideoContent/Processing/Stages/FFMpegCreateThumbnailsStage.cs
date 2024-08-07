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
			if (context == null)
			{
				throw new ArgumentNullException($"Context must not be null for stage {nameof(FFMpegCreateThumbnailsStage)}");
			}
			if (context.LocalCache == null)
			{
				throw new ArgumentNullException($"{nameof(context.RemoteCache)} must not be null for stage {nameof(FFMpegCreateThumbnailsStage)}");
			}

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
			if (context.LocalCache == null)
			{
				throw new ArgumentNullException($"{nameof(context.RemoteCache)} must not be null for stage {nameof(FFMpegCreateThumbnailsStage)}");
			}
			if (context.LocalCache.QualityTierPaths == null || context.LocalCache.QualityTierPaths.Count == 0)
			{
				throw new ArgumentNullException($"{context.LocalCache.QualityTierPaths} must not be null or empty for stage {nameof(FFMpegCreateThumbnailsStage)}");
			}
			VideoProcessingOptions options = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
			return context.LocalCache.QualityTierPaths.Last(_ => _.Key <= options.ThumbnailsQualityTier).Value;
		}
    }
}
