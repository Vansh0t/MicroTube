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
			if (context == null)
			{
				throw new ArgumentNullException($"Context must not be null for stage {nameof(OfflineUploadVideoToCdnStage)}");
			}
			if (context.LocalCache == null)
			{
				throw new ArgumentNullException($"{nameof(context.LocalCache)} must not be null for stage {nameof(OfflineUploadVideoToCdnStage)}");
			}
			if (context.RemoteCache == null)
			{
				throw new ArgumentNullException($"{nameof(context.RemoteCache)} must not be null for stage {nameof(OfflineUploadVideoToCdnStage)}");
			}
			var videoCdnUrl = await UploadVideoToCdn(context.LocalCache.SourcePath, context.RemoteCache.VideoFileName, cancellationToken);
			if (context.Cdn == null)
				context.Cdn = new Cdn();
			context.Cdn.VideoEndpoint = videoCdnUrl;
			return context;
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
