using Azure.Storage.Blobs.Models;

namespace MicroTube.Services.MediaContentStorage
{
	public class AzureBlobUploadOptions : IRemoteUploadOptions
	{
		public string FileName { get; set; }
		public string ContainerName { get; set; }
		public BlobUploadOptions BlobUploadOptions { get; set; }
		public AzureBlobUploadOptions(string fileName, string containerName, BlobUploadOptions blobUploadOptions)
		{
			FileName = fileName;
			ContainerName = containerName;
			BlobUploadOptions = blobUploadOptions;
		}
	}
}
