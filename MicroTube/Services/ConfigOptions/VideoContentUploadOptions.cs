namespace MicroTube.Services.ConfigOptions
{
	public class VideoContentUploadOptions
	{
		public static string KEY = "VideoContentUpload";
		public string CdnUrl { get; set; }

		public VideoContentUploadOptions(string cdnUrl)
		{
			CdnUrl = cdnUrl;
		}
	}
}
