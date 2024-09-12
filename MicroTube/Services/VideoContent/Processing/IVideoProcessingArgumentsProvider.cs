namespace MicroTube.Services.VideoContent.Processing
{
	public interface IVideoProcessingArgumentsProvider
	{
		string ProvideForCompression(string fileExtension);
		string ProvideForThumbnails(string fileExtension);
	}
}