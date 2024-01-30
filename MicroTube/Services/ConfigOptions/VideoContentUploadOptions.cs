namespace MicroTube.Services.ConfigOptions
{
	public class VideoContentUploadOptions
	{
		public static string KEY = "VideoContentUpload";
		public string AbsoluteLocalStoragePath { get; set; }
		public long MaxFileSizeBytes { get; set; }
		public int LocalStorageUploadBufferSizeBytes { get; set; }
		public HashSet<string> AllowedContentTypes { get; set; } = new HashSet<string>();
		public HashSet<string> AllowedFileExtensions { get; set; } = new HashSet<string>();
		public VideoContentUploadOptions(string absoluteLocalStoragePath, long maxFileSizeBytes, int localStorageUploadBufferSizeBytes)
		{
			AbsoluteLocalStoragePath = absoluteLocalStoragePath;
			MaxFileSizeBytes = maxFileSizeBytes;
			LocalStorageUploadBufferSizeBytes = localStorageUploadBufferSizeBytes;
		}
	}
}
