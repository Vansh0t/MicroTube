namespace MicroTube.Data.Models
{
	public class VideoUploadProgress
	{
		public Guid Id { get; set; }
		public VideoUploadStatus Status { get; set; }
		public Guid UploaderId { get; set; }
		public required string Title { get; set; }
		public string? Description { get; set; }
		public required string RemoteCacheLocation { get; set; }
		public required string RemoteCacheFileName { get; set; }
		public string? Message { get; set; }
	}
	public enum VideoUploadStatus {InQueue, InProgress, Fail, Success}
}
