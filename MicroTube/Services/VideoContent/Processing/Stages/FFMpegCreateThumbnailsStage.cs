using MicroTube.Services.ConfigOptions;
using static Org.BouncyCastle.Math.EC.ECCurve;

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
			VideoProcessingOptions options = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
			ValidateContext(context);
            string localCacheSourcePath = Path.Join(context!.LocalCache!.VideoFileLocation, context.LocalCache.VideoFileName);
            var thumbnailPaths = await MakeThumbnails(localCacheSourcePath, options.AbsoluteLocalStoragePath, cancellationToken);
            context.Subcontent = new Subcontent
            {
                ThumbnailPaths = thumbnailPaths
            };
            return context;
        }
        private void ValidateContext(DefaultVideoProcessingContext? context)
        {
            if (context == null)
            {
                throw new ArgumentNullException($"Context must not be null for stage {nameof(FFMpegCreateThumbnailsStage)}");
            }
            if (context.LocalCache == null)
            {
                throw new ArgumentNullException($"{nameof(context.RemoteCache)} must not be null for stage {nameof(FFMpegCreateThumbnailsStage)}");
            }
            if (string.IsNullOrWhiteSpace(Path.Join(context.LocalCache.VideoFileLocation, context.LocalCache.VideoFileName)))
            {
                throw new ArgumentNullException($"Joined local cache source path must not be null for stage {nameof(FFMpegCreateThumbnailsStage)}");
            }
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
    }
}
