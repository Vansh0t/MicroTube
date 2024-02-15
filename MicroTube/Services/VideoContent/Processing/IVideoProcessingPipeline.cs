namespace MicroTube.Services.VideoContent.Processing
{
	public interface IVideoProcessingPipeline
	{
		Task<IServiceResult> Process(string videoFilePath, CancellationToken cancellationToken);
	}
}