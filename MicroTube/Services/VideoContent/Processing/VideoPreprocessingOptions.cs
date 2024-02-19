namespace MicroTube.Services.VideoContent.Processing
{
	public class VideoPreprocessingOptions
	{
		public string UserId { get; set; }
		public string VideoTitle { get; set; }
		public string? VideoDescription { get; set; }
		public IFormFile VideoFile { get; set; }
		public VideoPreprocessingOptions(string userId, string videoTitle, string? videoDescription, IFormFile videoFile)
		{
			UserId = userId;
			VideoTitle = videoTitle;
			VideoDescription = videoDescription;
			VideoFile = videoFile;
		}
	}
}
