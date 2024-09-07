namespace MicroTube.Services.Base
{
	public abstract class IsolatedExecutionPipeline<TContext> : IPipeline<TContext>
	{
		public PipelineState State { get; protected set; }
		protected readonly List<IPipelineStage<TContext>> stages = new();


		public abstract Task<TContext> Execute(TContext? context, CancellationToken cancellationToken = default);
		public void AddStage(IPipelineStage<TContext> stage)
		{
			ThrowIfExecuting();
			stages.Add(stage);
		}
		public void InsertStage(IPipelineStage<TContext> stage, int index)
		{
			ThrowIfExecuting();
			stages.Insert(index, stage);
		}

		public void RemoveStage(IPipelineStage<TContext> stage)
		{
			ThrowIfExecuting();
			stages.Remove(stage);
		}
		public void RemoveStageAt(int index)
		{
			ThrowIfExecuting();
			stages.RemoveAt(index);
		}
		protected void ThrowIfExecuting()
		{
			if (State == PipelineState.Executing)
				throw new BackgroundJobException("Operation is not allowed while the pipeline is executing");
		}
	}
}
