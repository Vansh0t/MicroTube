namespace MicroTube.Services.ConfigOptions
{
	public class VideoContentUploadOptions
	{
		public static string KEY = "VideoContentUpload";
		public string RemoteStorageLocation { get; set; }
		public string CdnUrl { get; set; }
		public string ThumbnailsStorageLocation { get; set; }

		public VideoContentUploadOptions(string remoteStorageLocation, string cdnUrl, string thumbnailsStorageLocation)
		{
			RemoteStorageLocation = remoteStorageLocation;
			CdnUrl = cdnUrl;
			ThumbnailsStorageLocation = thumbnailsStorageLocation;
		}
	}
}
