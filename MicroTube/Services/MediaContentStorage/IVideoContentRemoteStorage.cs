namespace MicroTube.Services.MediaContentStorage
{
	public interface IVideoContentRemoteStorage<TAccessOptions, TUploadOptions>
	{
		Task<IServiceResult<string>> Upload(Stream stream, TAccessOptions accessOptions, TUploadOptions uploadOptions, CancellationToken cancellationToken = default);
		Task<IServiceResult> Delete(TAccessOptions options, CancellationToken cancellationToken = default);
		Task<IServiceResult<string>> Download(string saveToPath, TAccessOptions options, CancellationToken cancellationToken = default);
		Task<IServiceResult<IEnumerable<string>>> Upload(string path, TAccessOptions accessOptions, TUploadOptions uploadOptions, CancellationToken cancellationToken = default);
		Task<IServiceResult> EnsureLocation(string locationName, RemoteLocationAccess locationAccess, CancellationToken cancellationToken = default);
		Task<IServiceResult> DeleteLocation(string locationName, CancellationToken cancellationToken = default);
	}
	public enum RemoteLocationAccess
	{
		Private,
		Public
	}
}