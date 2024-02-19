namespace MicroTube.Services.VideoContent.Processing
{
	public interface IVideoProcessingPipeline
	{
		Task Process(string videoFileName, string videoFileLocation, CancellationToken cancellationToken = default);
	}
}