namespace MicroTube.Services.ConfigOptions
{
	public class VideoProcessingOptions
	{
		public const string KEY = "VideoProcessing";
		public string RemoteStorageCacheLocation { get; set; }
		public int RemoteStorageCacheUploadBufferSizeBytes { get; set; }
		public int RemoteStorageCacheDownloadBufferSizeBytes { get; set; }
		public string AbsoluteLocalStoragePath { get; set; }
		public long MaxFileSizeBytes { get; set; }
		public int SnapshotsIntervalSeconds { get; set; }
		public int SnapshotsWidth{ get; set; }
		public int SnapshotsHeight { get; set; }
		public int SnapshotsQualityTier { get; set; }
		public int ThumbnailsAmount { get; set; }
		public int ThumbnailsWidth { get; set; }
		public int ThumbnailsHeight { get; set; }
		public int ThumbnailsQualityTier { get; set; }
		public List<int> QualityTiers { get; set; } = new List<int>();
		public HashSet<string> AllowedContentTypes { get; set; } = new HashSet<string>();
		public HashSet<string> AllowedFileExtensions { get; set; } = new HashSet<string>();
		public VideoProcessingOptions(
			string remoteStorageCacheLocation,
			int remoteStorageCacheUploadBufferSizeBytes,
			int remoteStorageCacheDownloadBufferSizeBytes,
			string absoluteLocalStoragePath,
			long maxFileSizeBytes,
			int snapshotsIntervalSeconds,
			int snapshotsWidth,
			int snapshotsHeight,
			int thumbnailsAmount,
			int thumbnailsWidth,
			int thumbnailsHeight,
			int thumbnailsQualityTier,
			int snapshotsQualityTier)
		{
			RemoteStorageCacheLocation = remoteStorageCacheLocation;
			RemoteStorageCacheUploadBufferSizeBytes = remoteStorageCacheUploadBufferSizeBytes;
			RemoteStorageCacheDownloadBufferSizeBytes = remoteStorageCacheDownloadBufferSizeBytes;
			AbsoluteLocalStoragePath = absoluteLocalStoragePath;
			MaxFileSizeBytes = maxFileSizeBytes;
			SnapshotsIntervalSeconds = snapshotsIntervalSeconds;
			SnapshotsWidth = snapshotsWidth;
			SnapshotsHeight = snapshotsHeight;
			ThumbnailsAmount = thumbnailsAmount;
			ThumbnailsWidth = thumbnailsWidth;
			ThumbnailsHeight = thumbnailsHeight;
			ThumbnailsQualityTier = thumbnailsQualityTier;
			SnapshotsQualityTier = snapshotsQualityTier;
		}
	}
}
