using Ardalis.GuardClauses;
using MicroTube.Services.ContentStorage;

namespace MicroTube.Services.VideoContent.Processing.Stages.Azure
{
	public class AzureUploadThumbnailsToCdnStage: VideoProcessingStage
	{
		private readonly ICdnMediaContentAccess _mediaCdnAccess;

		public AzureUploadThumbnailsToCdnStage(ICdnMediaContentAccess mediaCdnAccess)
		{
			_mediaCdnAccess = mediaCdnAccess;
		}

		protected override async Task<DefaultVideoProcessingContext> ExecuteInternal(DefaultVideoProcessingContext? context, CancellationToken cancellationToken)
		{
			Guard.Against.Null(context);
			Guard.Against.Null(context.LocalCache);
			Guard.Against.Null(context.RemoteCache);
			var thumbnailEndpoints = await UploadThumbnailsToCdn(context.LocalCache.ThumbnailsLocation, context.RemoteCache.VideoFileLocation, cancellationToken);
			if (context.Cdn == null)
				context.Cdn = new Cdn();
			context.Cdn.ThumbnailEndpoints = thumbnailEndpoints;
			return context;
		}
		private async Task<IEnumerable<Uri>> UploadThumbnailsToCdn(string localCacheThumbnailsLocation, string remoteLocation, CancellationToken cancellationToken)
		{
			var subcontentUploadResult = await _mediaCdnAccess.UploadVideoThumbnails(localCacheThumbnailsLocation, remoteLocation, cancellationToken);
			if (subcontentUploadResult.IsError)
			{
				throw new BackgroundJobException($"Failed to upload video subcontent to CDN. File: {remoteLocation}, SubcontentLocation: {localCacheThumbnailsLocation}. {subcontentUploadResult.Error}");
			}
			return subcontentUploadResult.GetRequiredObject();
		}
	}
}
