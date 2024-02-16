namespace MicroTube.Services.ConfigOptions
{
	public class VideoProcessingOptions
	{
		public const string KEY = "VideoProcessing";
		public string RemoteStorageCacheLocation { get; set; }
		public int RemoteStorageCacheUploadBufferSizeBytes { get; set; }
		public int RemoteStorageCacheDownloadBufferSizeBytes { get; set; }
		public VideoProcessingOptions(string remoteStorageCacheLocation, int remoteStorageCacheUploadBufferSizeBytes, int remoteStorageCacheDownloadBufferSizeBytes)
		{
			RemoteStorageCacheLocation = remoteStorageCacheLocation;
			RemoteStorageCacheUploadBufferSizeBytes = remoteStorageCacheUploadBufferSizeBytes;
			RemoteStorageCacheDownloadBufferSizeBytes = remoteStorageCacheDownloadBufferSizeBytes;
		}
	}
}
