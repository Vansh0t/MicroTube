using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using MicroTube.Services.ConfigOptions;

namespace MicroTube.Services.MediaContentStorage
{
	public class AzureBlobVideoContentRemoteStorage : IVideoContentRemoteStorage
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
		public async Task<IServiceResult> Upload(Stream stream, string fileName, CancellationToken cancellationToken)
		{
			var uploadOptions = _config.GetRequiredSection(VideoContentUploadOptions.KEY).GetRequired<VideoContentUploadOptions>();
			var blobContainerClient = _azureBlobServiceClient.GetBlobContainerClient(uploadOptions.RemoteStoragePath);
			var blobClient = blobContainerClient.GetBlockBlobClient(fileName);
			var response = await blobClient.UploadAsync(stream, 
				new BlobUploadOptions { AccessTier = AccessTier.Hot, HttpHeaders = new BlobHttpHeaders {ContentType = "video/mp4" } }, 
				cancellationToken);
			var httpResponse = response.GetRawResponse();
			if (httpResponse.IsError)
			{
				_logger.LogError("Failed to upload media content to Azure blob storage. {statusCode}, {reasonPhrase}", httpResponse.Status, httpResponse.ReasonPhrase);
				return ServiceResult.FailInternal();
			}
			return ServiceResult.Success();
		}
	}
}
