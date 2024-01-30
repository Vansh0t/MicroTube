using Azure.Storage.Blobs;

namespace MicroTube.Services.MediaContentStorage
{
	public class AzureCdnMediaContentAccess
	{
		private readonly IConfiguration _config; 
		private readonly BlobContainerClient _azureBlobs;
		private readonly ILogger<AzureCdnMediaContentAccess> _logger;

		public AzureCdnMediaContentAccess(BlobContainerClient azureBlobs, ILogger<AzureCdnMediaContentAccess> logger, IConfiguration config)
		{
			_azureBlobs = azureBlobs;
			_logger = logger;
			_config = config;
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
