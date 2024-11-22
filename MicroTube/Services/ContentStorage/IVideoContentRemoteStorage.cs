namespace MicroTube.Services.ContentStorage
{
	public interface IRemoteStorage<TAccessOptions, TUploadOptions>
	{
		Task<string> Upload(Stream stream, TAccessOptions accessOptions, TUploadOptions uploadOptions, CancellationToken cancellationToken = default);
		Task<string> Upload(string path, TAccessOptions accessOptions, TUploadOptions uploadOptions, CancellationToken cancellationToken = default);
		Task<BulkUploadResult> UploadDirectory(string directoryPath, TAccessOptions accessOptions, TUploadOptions uploadOptions, CancellationToken cancellationToken = default);
		Task Delete(TAccessOptions options, CancellationToken cancellationToken = default);
		Task<string> Download(string saveToPath, TAccessOptions options, CancellationToken cancellationToken = default);
		Task EnsureLocation(string locationName, RemoteLocationAccess locationAccess, CancellationToken cancellationToken = default);
		Task DeleteLocation(string locationName, CancellationToken cancellationToken = default);
		Task<bool> FileExists(TAccessOptions accessOptions, CancellationToken cancellationToken = default);
		Task<IDictionary<string, string?>> GetFileMetadata (TAccessOptions accessOptions, CancellationToken cancellationToken = default);
		Task<IDictionary<string, string?>> GetLocationMetadata(string locationName, CancellationToken cancellationToken = default);
	}
	public enum RemoteLocationAccess
	{
		Private,
		Public
	}
}