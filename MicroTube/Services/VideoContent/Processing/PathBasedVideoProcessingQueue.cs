namespace MicroTube.Services.VideoContent.Processing
{
	public class PathBasedVideoProcessingQueue : IVideoProcessingQueue
	{
		private readonly Queue<string> _processingQueue = new Queue<string>();
		public void EnqueueForProcessing(string fullFilePath)
		{
			if (!_processingQueue.Contains(fullFilePath))
				_processingQueue.Enqueue(fullFilePath);
		}
		public bool TryDequeue(out string? fullFilePath)
		{
			return _processingQueue.TryDequeue(out fullFilePath);
		}
	}
}
