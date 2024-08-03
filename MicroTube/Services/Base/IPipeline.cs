namespace MicroTube.Services.Base
{
	public interface IPipeline<TContext>
	{
		public PipelineState State { get; }
		public void AddStage(IPipelineStage<TContext> stage);
		public void InsertStage(IPipelineStage<TContext> stage, int index);
		public void RemoveStage(IPipelineStage<TContext> stage);
		public void RemoveStageAt(int index);

		public Task<TContext> Execute(TContext? context, CancellationToken cancellationToken = default);
	}
	public enum PipelineState
	{
		Idle,
		Executing,
		Finished,
		Error
	}
}
