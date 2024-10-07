using MicroTube.Services.Base;
using MicroTube.Services.VideoContent.Processing.Stages;

namespace MicroTube.Services.VideoContent.Preprocessing.Stages
{
	public abstract class VideoPreprocessingStage: IPipelineStage<DefaultVideoPreprocessingContext>
	{
		public PipelineStageState State => state;
		protected PipelineStageState state;

		public async Task<DefaultVideoPreprocessingContext> Execute(DefaultVideoPreprocessingContext? context, CancellationToken cancellationToken = default)
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
		protected abstract Task<DefaultVideoPreprocessingContext> ExecuteInternal(
			DefaultVideoPreprocessingContext? context,
			CancellationToken cancellationToken);
	}
}
