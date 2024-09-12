namespace MicroTube.Services.VideoContent.Processing
{
	public interface IVideoThumbnailsService
	{
		Task<IServiceResult<IEnumerable<string>>> MakeSnapshots(string filePath, string saveToPath, CancellationToken cancellationToken = default);
		Task<IServiceResult<IEnumerable<string>>> MakeThumbnails(string filePath, string saveToPath, CancellationToken cancellationToken = default);
	}
}