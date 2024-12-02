namespace MicroTube.Services.VideoContent.Preprocessing
{
    public class VideoPreprocessingData
    {
		public string UserId { get; set; }
		public string GeneratedSourceFileName { get; set; }
		public string GeneratedSourceFileLocation { get; set; }
		public VideoPreprocessingData(string userId, string generatedSourceFileName, string generatedSourceFileLocation)
		{
			UserId = userId;
			GeneratedSourceFileName = generatedSourceFileName;
			GeneratedSourceFileLocation = generatedSourceFileLocation;
		}
	}
}
