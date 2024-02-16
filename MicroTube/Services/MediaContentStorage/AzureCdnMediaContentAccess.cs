namespace MicroTube.Services.MediaContentStorage
{
	public class AzureCdnMediaContentAccess : ICdnMediaContentAccess
	{
		private readonly IVideoContentRemoteStorage<AzureBlobUploadOptions> _videoRemoteStorage;

		public AzureCdnMediaContentAccess(IVideoContentRemoteStorage<AzureBlobUploadOptions> videoRemoteStorage)
		{
			_videoRemoteStorage = videoRemoteStorage;
		}

		public async Task<IServiceResult> UploadVideo(Stream stream, string fileName, CancellationToken cancellationToken = default)
		{
			//return await _videoRemoteStorage.Upload(stream, fileName, cancellationToken);
			return ServiceResult.Success();
		}
	}
}
