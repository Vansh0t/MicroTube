using Azure.Storage.Blobs;

namespace MicroTube.Services.MediaContentStorage
{
	public class AzureCdnMediaContentAccess
	{
		private BlobContainerClient _azureBlobs;
		private ILogger<AzureCdnMediaContentAccess> _logger;

		public AzureCdnMediaContentAccess(BlobContainerClient azureBlobs, ILogger<AzureCdnMediaContentAccess> logger)
		{
			_azureBlobs = azureBlobs;
			_logger = logger;
		}

		public async Task<IServiceResult> Upload(Stream stream, string fileName, CancellationToken cancellationToken = default)
		{
			var response = await _azureBlobs.UploadBlobAsync(fileName, stream, cancellationToken);
			var httpResponse = response.GetRawResponse();
			if(httpResponse.IsError)
			{
				_logger.LogError("Failed to upload media content to Azure blob storage. {statusCode}, {reasonPhrase}", httpResponse.Status, httpResponse.ReasonPhrase);
				return ServiceResult.FailInternal();
			}
			return ServiceResult.Success();
		}
	}
}
