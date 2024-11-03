namespace MicroTube.Services.Search.Videos
{
	public interface IVideoIndexingService
	{
		Task EnsureVideoIndices();
	}
}