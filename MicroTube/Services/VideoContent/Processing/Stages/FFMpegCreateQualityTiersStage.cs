using Ardalis.GuardClauses;
using MicroTube.Services.ConfigOptions;

namespace MicroTube.Services.VideoContent.Processing.Stages
{
	public class FFMpegCreateQualityTiersStage : VideoProcessingStage
	{
		private readonly IVideoCompressionService _compressionService;
		private readonly IConfiguration _config;

		public FFMpegCreateQualityTiersStage(IVideoCompressionService compressionService, IConfiguration config)
		{
			_compressionService = compressionService;
			_config = config;
		}

		protected override async Task<DefaultVideoProcessingContext> ExecuteInternal(DefaultVideoProcessingContext? context, CancellationToken cancellationToken)
		{
			Guard.Against.Null(context);
			Guard.Against.Null(context.LocalCache);
			var options = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
			var qualityTiersResult = await _compressionService.CompressToQualityTiers(
				options.QualityTiers,
				context.LocalCache.SourcePath,
				context.LocalCache.QualityTiersLocation,
				cancellationToken);
			if (qualityTiersResult.IsError)
				throw new BackgroundJobException($"Failed to make quality tiers. {qualityTiersResult.Error}");
			context.LocalCache.QualityTierPaths = qualityTiersResult.GetRequiredObject();
			return context;
		}
	}
}
