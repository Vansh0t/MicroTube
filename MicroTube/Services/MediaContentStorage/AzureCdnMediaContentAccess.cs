namespace MicroTube.Services.MediaContentStorage
{
	public class AzureCdnMediaContentAccess : ICdnMediaContentAccess
	{
		private readonly IVideoContentRemoteStorage _videoRemoteStorage;

		public AzureCdnMediaContentAccess(IVideoContentRemoteStorage videoRemoteStorage)
		{
			_videoRemoteStorage = videoRemoteStorage;
		}

		public async Task<IServiceResult> UploadVideo(Stream stream, string fileName, CancellationToken cancellationToken = default)
		{
			return await _videoRemoteStorage.Upload(stream, fileName, cancellationToken);
		}
	}
}
