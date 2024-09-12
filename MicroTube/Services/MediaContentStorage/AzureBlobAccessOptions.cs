using Azure.Storage.Blobs.Models;

namespace MicroTube.Services.MediaContentStorage
{
	public class AzureBlobAccessOptions
	{
		public string FileName { get; set; }
		public string ContainerName { get; set; }
		public AzureBlobAccessOptions(string fileName, string containerName)
		{
			FileName = fileName;
			ContainerName = containerName;
		}
	}
}
