namespace MicroTube.Services.MediaContentStorage
{
	public interface ICdnMediaContentAccess
	{
		Task<IServiceResult> DeleteAllVideoData(string videoFileName, CancellationToken cancellationToken = default);
		Task<IServiceResult<IEnumerable<Uri>>> UploadVideoQualityTiers(string tiersDirectory, string location, CancellationToken cancellationToken = default);
		Task<IServiceResult<IEnumerable<Uri>>> UploadVideoThumbnails(string thumbnailsDirectory, string location, CancellationToken cancellationToken = default);
		public Task<IServiceResult<string>> CreateVideoLocation(string videoFileName, CancellationToken cancellationToken = default);
	}
}