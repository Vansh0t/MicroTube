namespace MicroTube.Services.MediaContentStorage
{
	public interface IVideoContentRemoteStorage
	{
		Task<IServiceResult> Upload(Stream stream, string fileName, CancellationToken cancellationToken);
	}
}