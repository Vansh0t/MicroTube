﻿using MicroTube.Services.MediaContentStorage;

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
			if (context == null)
			{
				throw new ArgumentNullException($"Context must not be null for stage {nameof(OfflineUploadThumbnailsToCdnStage)}");
			}
			if (context.LocalCache == null)
			{
				throw new ArgumentNullException($"{nameof(context.LocalCache)} must not be null for stage {nameof(OfflineUploadThumbnailsToCdnStage)}");
			}
			if (context.RemoteCache == null)
			{
				throw new ArgumentNullException($"{nameof(context.RemoteCache)} must not be null for stage {nameof(OfflineUploadThumbnailsToCdnStage)}");
			}
			var thumbnailEndpoints = await UploadThumbnailsToCdn(context.LocalCache.ThumbnailsLocation, context.RemoteCache.VideoFileName, cancellationToken);
			if (context.Cdn == null)
				context.Cdn = new Cdn();
			context.Cdn.ThumbnailEndpoints = thumbnailEndpoints;
			return context;
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