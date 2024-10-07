namespace MicroTube.Services.VideoContent.Preprocessing
{
    public class VideoPreprocessingData
    {
        public string UserId { get; set; }
        public string VideoTitle { get; set; }
        public string? VideoDescription { get; set; }
        public IFormFile VideoFile { get; set; }
        public VideoPreprocessingData(string userId, string videoTitle, string? videoDescription, IFormFile videoFile)
        {
            UserId = userId;
            VideoTitle = videoTitle;
            VideoDescription = videoDescription;
            VideoFile = videoFile;
        }
    }
}
