using MicroTube.Services.MediaContentStorage;

namespace MicroTube.Services.VideoContent.Processing.Stages.Offline
{
	public class OfflineUploadThumbnailsToCdnStage: VideoProcessingStage
	{
		private readonly ICdnMediaContentAccess _mediaCdnAccess;

		public OfflineUploadThumbnailsToCdnStage(ICdnMediaContentAccess mediaCdnAccess)
		{
			_mediaCdnAccess = mediaCdnAccess;
		}

		protected override async Task<DefaultVideoProcessingContext> ExecuteInternal(DefaultVideoProcessingContext? context, CancellationToken cancellationToken)
		{
			ValidateContext(context);
			string thumbnail = context!.Subcontent!.ThumbnailPaths!.First();
			string? thumbnailsLocation = Path.GetDirectoryName(thumbnail);
			if(string.IsNullOrWhiteSpace(thumbnailsLocation))
			{
				throw new BackgroundJobException("Thumbnails containing location cannot be null or empty");
			}
			var thumbnailEndpoints = await UploadThumbnailsToCdn(thumbnailsLocation, context.SourceVideoNormalizedName!, cancellationToken);
			if (context.Cdn == null)
				context.Cdn = new Cdn();
			context.Cdn.ThumbnailEndpoints = thumbnailEndpoints;
			return context;
		}
		private void ValidateContext(DefaultVideoProcessingContext? context)
		{
			if (context == null)
			{
				throw new ArgumentNullException($"Context must not be null for stage {nameof(OfflineUploadThumbnailsToCdnStage)}");
			}
			if (context.SourceVideoNormalizedName == null)
			{
				throw new ArgumentNullException($"{nameof(context.RemoteCache.VideoFileName)} must not be null for stage {nameof(OfflineUploadThumbnailsToCdnStage)}");
			}
			if (context.Subcontent == null)
			{
				throw new ArgumentNullException($"{nameof(context.Subcontent)} must not be null for stage {nameof(OfflineUploadThumbnailsToCdnStage)}");
			}
			if (context.Subcontent.ThumbnailPaths == null || context.Subcontent.ThumbnailPaths.FirstOrDefault() == null)
			{
				throw new ArgumentNullException($"{nameof(context.Subcontent.ThumbnailPaths)} must not be null for stage {nameof(OfflineUploadThumbnailsToCdnStage)}");
			}
		}
		private async Task<IEnumerable<Uri>> UploadThumbnailsToCdn(string localCacheThumbnailsLocation, string normalizedSourceName, CancellationToken cancellationToken)
		{
			var subcontentUploadResult = await _mediaCdnAccess.UploadVideoSubcontent(localCacheThumbnailsLocation, normalizedSourceName, cancellationToken);
			if (subcontentUploadResult.IsError)
			{
				throw new BackgroundJobException($"Failed to upload video subcontent to CDN. File: {normalizedSourceName}, SubcontentLocation: {localCacheThumbnailsLocation}. {subcontentUploadResult.Error}");
			}
			return subcontentUploadResult.GetRequiredObject();
		}

	}
}
