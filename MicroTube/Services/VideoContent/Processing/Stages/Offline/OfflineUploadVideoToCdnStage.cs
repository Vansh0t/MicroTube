using MicroTube.Services.MediaContentStorage;

namespace MicroTube.Services.VideoContent.Processing.Stages.Offline
{
	public class OfflineUploadVideoToCdnStage : VideoProcessingStage
	{
		private readonly ICdnMediaContentAccess _mediaCdnAccess;

		public OfflineUploadVideoToCdnStage(ICdnMediaContentAccess mediaCdnAccess)
		{
			_mediaCdnAccess = mediaCdnAccess;
		}

		protected override async Task<DefaultVideoProcessingContext> ExecuteInternal(DefaultVideoProcessingContext? context, CancellationToken cancellationToken)
		{
			ValidateContext(context);
			string localCacheSourcePath = Path.Join(context!.LocalCache!.VideoFileLocation, context.LocalCache.VideoFileName);
			var videoCdnUrl = await UploadVideoToCdn(localCacheSourcePath, context.SourceVideoNormalizedName!, cancellationToken);
			if (context.Cdn == null)
				context.Cdn = new Cdn();
			context.Cdn.VideoEndpoint = videoCdnUrl;
			return context;
		}
		private void ValidateContext(DefaultVideoProcessingContext? context)
		{
			if (context == null)
			{
				throw new ArgumentNullException($"Context must not be null for stage {nameof(OfflineUploadVideoToCdnStage)}");
			}
			if (context.SourceVideoNormalizedName == null)
			{
				throw new ArgumentNullException($"{nameof(context.SourceVideoNormalizedName)} must not be null for stage {nameof(OfflineUploadVideoToCdnStage)}");
			}
			if (context.LocalCache == null)
			{
				throw new ArgumentNullException($"{nameof(context.RemoteCache)} must not be null for stage {nameof(OfflineUploadVideoToCdnStage)}");
			}
			if (string.IsNullOrWhiteSpace(Path.Join(context.LocalCache.VideoFileLocation, context.LocalCache.VideoFileName)))
			{
				throw new ArgumentNullException($"Joined local cache source path must not be null for stage {nameof(OfflineUploadVideoToCdnStage)}");
			}
		}
		private async Task<Uri> UploadVideoToCdn(string localCacheSourceVideoPath, string normalizedVideoName, CancellationToken cancellationToken)
		{
			using var fileStream = new FileStream(localCacheSourceVideoPath, FileMode.Open, FileAccess.Read, FileShare.Read);
			var videoUploadResult = await _mediaCdnAccess.UploadVideo(fileStream, normalizedVideoName, cancellationToken);
			if (videoUploadResult.IsError)
			{
				throw new BackgroundJobException($"Failed to upload video file to CDN. Path: {localCacheSourceVideoPath}. {videoUploadResult.Error}");
			}
			return videoUploadResult.GetRequiredObject();
		}
	}
}
