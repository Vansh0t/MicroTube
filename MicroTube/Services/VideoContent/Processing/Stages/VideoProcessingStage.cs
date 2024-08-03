using MicroTube.Services.Base;

namespace MicroTube.Services.VideoContent.Processing.Stages
{
	public abstract class VideoProcessingStage : IPipelineStage<DefaultVideoProcessingContext>
	{
		public PipelineStageState State => state;
		protected PipelineStageState state;

		public async Task<DefaultVideoProcessingContext> Execute(DefaultVideoProcessingContext? context, CancellationToken cancellationToken = default)
		{
			try
			{
				state = PipelineStageState.Executing;
				var result = await ExecuteInternal(context, cancellationToken);
				state = PipelineStageState.Finished;
				return result;
			}
			catch
			{
				state = PipelineStageState.Error;
				throw;
			}
		}
		protected abstract Task<DefaultVideoProcessingContext> ExecuteInternal(
			DefaultVideoProcessingContext? context,
			CancellationToken cancellationToken);
	}
}
