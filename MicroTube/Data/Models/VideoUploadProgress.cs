namespace MicroTube.Data.Models
{
	public class VideoUploadProgress
	{
		public Guid Id { get; set; }
		public required VideoUploadStatus Status { get; set; }
		public required Guid UploaderId { get; set; }
		public required string Title { get; set; }
		public string? Description { get; set; }
		public required string RemoteCacheLocation { get; set; }
		public required string RemoteCacheFileName { get; set; }
		public required DateTime Timestamp { get; set; }
		public string? Message { get; set; }
		public int? LengthSeconds { get; set; }
		public string? FrameSize { get; set; }
		public string? Format { get; set; }
		public int? Fps { get; set; }

	}
	public enum VideoUploadStatus {InQueue, InProgress, Fail, Success}
}
