namespace MicroTube.Services.MediaContentStorage
{
	public interface ICdnMediaContentAccess
	{
		Task<IServiceResult<IEnumerable<Uri>>> UploadVideoQualityTiers(string tiersDirectory, string location, CancellationToken cancellationToken = default);
		Task<IServiceResult<IEnumerable<Uri>>> UploadVideoThumbnails(string thumbnailsDirectory, string location, CancellationToken cancellationToken = default);
	}
}