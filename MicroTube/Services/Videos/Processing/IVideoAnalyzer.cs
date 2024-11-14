namespace MicroTube.Services.VideoContent.Processing
{
	public interface IVideoAnalyzer
	{
		Task<VideoFileMetaData> Analyze(string filePath, CancellationToken cancellationToken = default);
	}
}