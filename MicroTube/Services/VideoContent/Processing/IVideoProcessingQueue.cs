namespace MicroTube.Services.VideoContent.Processing
{
	public interface IVideoProcessingQueue
	{
		void EnqueueForProcessing(string fullFilePath);
		bool TryDequeue(out string? fullFilePath);
	}
}