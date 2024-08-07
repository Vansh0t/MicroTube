using MicroTube.Services.ConfigOptions;
using MicroTube.Services.VideoContent.Processing.Stages.Offline;

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
			if(context == null)
			{
				throw new ArgumentNullException($"Context must not be null for stage {nameof(FFMpegCreateQualityTiersStage)}");
			}
			if(context.LocalCache == null)
			{
				throw new ArgumentNullException($"{nameof(context.LocalCache)} must not be null for stage {nameof(FFMpegCreateQualityTiersStage)}");
			}
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
