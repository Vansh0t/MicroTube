using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
namespace MicroTube.Services.MediaContentStorage
{
	public class AzureBlobVideoContentRemoteStorage : IVideoContentRemoteStorage<AzureBlobUploadOptions>
	{
		private readonly IConfiguration _config;
		private readonly BlobServiceClient _azureBlobServiceClient;
		private readonly ILogger<AzureBlobVideoContentRemoteStorage> _logger;

		public AzureBlobVideoContentRemoteStorage(IConfiguration config, BlobServiceClient azureBlobServiceClient, ILogger<AzureBlobVideoContentRemoteStorage> logger)
		{
			_config = config;
			_azureBlobServiceClient = azureBlobServiceClient;
			_logger = logger;
		}
		public async Task<IServiceResult> Upload(Stream stream, AzureBlobUploadOptions uploadOptions, CancellationToken cancellationToken = default)
		{
			var blobContainerClient = _azureBlobServiceClient.GetBlobContainerClient(uploadOptions.ContainerName);
			var blobClient = blobContainerClient.GetBlockBlobClient(uploadOptions.FileName);
			var response = await blobClient.UploadAsync(stream, uploadOptions.BlobUploadOptions, cancellationToken);
			var httpResponse = response.GetRawResponse();
			if (httpResponse.IsError)
			{
				_logger.LogError("Failed to upload media content to Azure blob storage. {statusCode}, {reasonPhrase}", httpResponse.Status, httpResponse.ReasonPhrase);
				return ServiceResult.FailInternal();
			}
			return ServiceResult.Success();
		}
		public async Task<IServiceResult> Delete(AzureBlobUploadOptions deleteOptions, CancellationToken cancellationToken = default)
		{
			var blobContainerClient = _azureBlobServiceClient.GetBlobContainerClient(deleteOptions.ContainerName);
			var blobClient = blobContainerClient.GetBlockBlobClient(deleteOptions.FileName);
			var response = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
			var httpResponse = response.GetRawResponse();
			if (httpResponse.IsError)
			{
				_logger.LogError("Failed to delete media content from Azure blob storage. {statusCode}, {reasonPhrase}", httpResponse.Status, httpResponse.ReasonPhrase);
				return ServiceResult.FailInternal();
			}
			return ServiceResult.Success();
		}
	}
}
