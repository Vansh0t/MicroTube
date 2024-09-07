using Ardalis.GuardClauses;
using Hangfire;
using MicroTube.Services.VideoContent.Processing;
using MicroTube.Services.VideoContent.Processing.Stages;

namespace MicroTube.Services.VideoContent.Preprocessing.Stages
{
	public class MakeProcessingJobStage : VideoPreprocessingStage
	{
		private readonly IBackgroundJobClient _jobClient;

		public MakeProcessingJobStage(IBackgroundJobClient jobClient)
		{
			_jobClient = jobClient;
		}

		protected override Task<DefaultVideoPreprocessingContext> ExecuteInternal(DefaultVideoPreprocessingContext? context, CancellationToken cancellationToken)
		{
			Guard.Against.Null(context);
			Guard.Against.Null(context.RemoteCache);
			var processingContext = new DefaultVideoProcessingContext() { RemoteCache = context.RemoteCache };
			_jobClient.Enqueue<IVideoProcessingPipeline>("video_processing", processing => processing.Execute(processingContext, default));
			return Task.FromResult(context);
		}
	}
}
