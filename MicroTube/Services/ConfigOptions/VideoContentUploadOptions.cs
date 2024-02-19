namespace MicroTube.Services.ConfigOptions
{
	public class VideoContentUploadOptions
	{
		public static string KEY = "VideoContentUpload";
		public string RemoteStorageLocation { get; set; }
		public string CdnUrl { get; set; }

		public VideoContentUploadOptions(string remoteStorageLocation, string cdnUrl)
		{
			RemoteStorageLocation = remoteStorageLocation;
			CdnUrl = cdnUrl;
		}
	}
}
