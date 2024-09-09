using Ardalis.GuardClauses;
using MicroTube.Services.ContentStorage;
using System.IO.Abstractions;

namespace MicroTube.Services.VideoContent.Processing.Stages.Azure
{
	public class AzureUploadVideoToCdnStage: VideoProcessingStage
	{
		private readonly ICdnMediaContentAccess _mediaCdnAccess;
		private readonly IFileSystem _fileSystem;

		public AzureUploadVideoToCdnStage(ICdnMediaContentAccess mediaCdnAccess, IFileSystem fileSystem)
		{
			_mediaCdnAccess = mediaCdnAccess;
			_fileSystem = fileSystem;
		}

		protected override async Task<DefaultVideoProcessingContext> ExecuteInternal(DefaultVideoProcessingContext? context, CancellationToken cancellationToken)
		{
			Guard.Against.Null(context);
			Guard.Against.Null(context.LocalCache);
			Guard.Against.Null(context.RemoteCache);
			IEnumerable<Uri> videoUrls = await UploadVideoTiersToCdn(context.LocalCache.QualityTiersLocation, context.RemoteCache.VideoFileLocation, cancellationToken);
			if (context.Cdn == null)
				context.Cdn = new Cdn();
			context.Cdn.VideoEndpoints = videoUrls;
			return context;
		}
		private async Task<IEnumerable<Uri>> UploadVideoTiersToCdn(string tiersDirectory, string location, CancellationToken cancellationToken)
		{
			var videoUploadResult = await _mediaCdnAccess.UploadVideoQualityTiers(tiersDirectory, location, cancellationToken);
			if (videoUploadResult.IsError || videoUploadResult.Code == 207) //207 means some quality tiers were not uploaded, consider it fatal error for now
			{
				throw new BackgroundJobException($"Failed to upload video quality tiers to CDN. Path: {tiersDirectory}. {videoUploadResult.Error ?? "Partial success is not not allowed."}");
			}
			return videoUploadResult.GetRequiredObject();
		}
	}
}
