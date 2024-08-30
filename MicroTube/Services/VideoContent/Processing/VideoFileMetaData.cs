namespace MicroTube.Services.VideoContent.Processing
{
	public class VideoFileMetaData
	{
		public string? Format { get; set; }
		public int FrameCount { get; set; }
		public int LengthSeconds { get; set; }
		public float Fps { get; set; }
		public required string FrameSize { get; set; } 
		public int FrameHeight
		{
			get
			{
				if (string.IsNullOrWhiteSpace(FrameSize))
					return 0;
				var split = FrameSize.Split("x");
				if (split.Length < 2)
					return 0;
				if (!int.TryParse(split.Last(), out var height))
					return 0;
				return height;
			}
		}
	}
}
