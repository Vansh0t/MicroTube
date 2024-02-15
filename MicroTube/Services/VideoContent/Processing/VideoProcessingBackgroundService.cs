namespace MicroTube.Services.VideoContent.Processing
{
	public class VideoProcessingBackgroundService : BackgroundService
	{
		private readonly ILogger<VideoProcessingBackgroundService> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly IVideoProcessingQueue _processingQueue;
		private readonly HashSet<Task<IServiceResult>> _activeProcessing = new HashSet<Task<IServiceResult>>();
		private readonly PeriodicTimer _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000));

		public VideoProcessingBackgroundService(
			ILogger<VideoProcessingBackgroundService> logger,
			IServiceProvider serviceProvider,
			IVideoProcessingQueue processingQueue)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
			_processingQueue = processingQueue;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(await _timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
			{
				HandleActiveProcessing();
				HandleNewProcessing(stoppingToken);
			}
			_timer.Dispose();
			HandleActiveProcessing();
			HandleNewProcessing(stoppingToken);
			_logger.LogInformation($"Waiting for {_activeProcessing.Count} video processing tasks to complete...");
			await Task.WhenAll(_activeProcessing);
		}
		
		private async Task<IServiceResult> ProcessVideo(string fullFilePath, CancellationToken cancellationToken)
		{
			var scope = _serviceProvider.CreateScope();
			IVideoProcessingPipeline processingPipeline = scope.ServiceProvider.GetRequiredService<IVideoProcessingPipeline>();
			_logger.LogInformation("Processing a new video file at path {fullFilePath}", fullFilePath);
			return await processingPipeline.Process(fullFilePath, cancellationToken);
		}
		private void HandleActiveProcessing()
		{
			var completedTasks = _activeProcessing.Where(_ => _.IsCompleted);
			var completedCount = _activeProcessing.RemoveWhere(_ => _.IsCompleted);
			if(completedCount > 0)
				_logger.LogInformation("Completed processing {completedTasksCount} videos", completedCount);
		}
		private void HandleNewProcessing(CancellationToken cancellationToken)
		{
			while (_processingQueue.TryDequeue(out var fullFilePath))
			{
				var newProcessingTask = ProcessVideo(fullFilePath, cancellationToken);
				_activeProcessing.Add(newProcessingTask);
			}
		}
	}
}
