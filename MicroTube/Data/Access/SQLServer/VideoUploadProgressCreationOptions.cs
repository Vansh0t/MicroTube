namespace MicroTube.Data.Access.SQLServer
{
	public class VideoUploadProgressCreationOptions
	{
		public string UploaderId { get; set; }
		public string RemoteCacheLocation { get; set; }
		public string RemoteCacheFileName { get; set; }
		public string Title { get; set; }
		public DateTime Timestamp { get; set; }
		public string? Description { get; set; }
		public int? LengthSeconds { get; set; }
		public int? Fps { get; set; }
		public string? FrameSize { get; set; }
		public string? Format { get; set; }
		public VideoUploadProgressCreationOptions(
			string uploaderId,
			string remoteCacheLocation,
			string remoteCacheFileName,
			string title,
			DateTime timestamp,
			string? description,
			int? lengthSeconds = null,
			int? fps = null,
			string? frameSize = null,
			string? format = null)
		{
			UploaderId = uploaderId;
			RemoteCacheLocation = remoteCacheLocation;
			RemoteCacheFileName = remoteCacheFileName;
			Title = title;
			Description = description;
			LengthSeconds = lengthSeconds;
			Fps = fps;
			FrameSize = frameSize;
			Format = format;
			Timestamp = timestamp;
		}
	}
}
