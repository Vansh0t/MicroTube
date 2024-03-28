namespace MicroTube.Services.VideoContent.Processing
{
	public class VideoFileMetaData
	{
		public string? Format { get; set; }
		public int FrameCount { get; set; }
		public int LengthSeconds { get; set; }
		public float Fps { get; set; }
		public string? FrameSize { get; set; } 
	}
}
