namespace MicroTube.Services.Base
{
	public interface IPipelineStage<T>
	{
		public PipelineStageState State { get; }
		public Task<T> Execute(T? context, CancellationToken cancellationToken = default);
	}
	public enum PipelineStageState
	{
		Idle,
		Executing,
		Finished,
		Error
	}
}
