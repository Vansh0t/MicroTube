namespace MicroTube.Services.MediaContentStorage
{
	public interface IVideoContentRemoteStorage<TOptions>
	{
		Task<IServiceResult> Upload(Stream stream, TOptions options, CancellationToken cancellationToken = default);
		Task<IServiceResult> Delete(TOptions options, CancellationToken cancellationToken = default);
	}
}