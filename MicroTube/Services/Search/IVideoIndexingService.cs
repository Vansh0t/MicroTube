namespace MicroTube.Services.Search
{
	public interface IVideoIndexingService
	{
		Task EnsureVideoIndices();
	}
}