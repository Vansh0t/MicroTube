namespace MicroTube.Services.ConfigOptions
{
	public class VideoContentUploadOptions
	{
		public static string KEY = "VideoContentUpload";
		public string CdnUrl { get; set; }
		public int DirectLinkLifetimeMinutes { get; set; }
		public VideoContentUploadOptions(string cdnUrl, int directLinkLifetimeMinutes)
		{
			CdnUrl = cdnUrl;
			DirectLinkLifetimeMinutes = directLinkLifetimeMinutes;
		}
	}
}
