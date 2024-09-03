using Azure.Storage.Blobs.Models;

namespace MicroTube.Services.MediaContentStorage
{
	public interface IVideoContentRemoteStorage<TAccessOptions, TUploadOptions>
	{
		Task<string> Upload(Stream stream, TAccessOptions accessOptions, TUploadOptions uploadOptions, CancellationToken cancellationToken = default);
		Task<string> Upload(string path, TAccessOptions accessOptions, TUploadOptions uploadOptions, CancellationToken cancellationToken = default);
		Task<BulkUploadResult> UploadDirectory(string directoryPath, TAccessOptions accessOptions, TUploadOptions uploadOptions, CancellationToken cancellationToken = default);
		Task Delete(TAccessOptions options, CancellationToken cancellationToken = default);
		Task<string> Download(string saveToPath, TAccessOptions options, CancellationToken cancellationToken = default);
		Task EnsureLocation(string locationName, RemoteLocationAccess locationAccess, CancellationToken cancellationToken = default);
		Task DeleteLocation(string locationName, CancellationToken cancellationToken = default);
	}
	public enum RemoteLocationAccess
	{
		Private,
		Public
	}
}