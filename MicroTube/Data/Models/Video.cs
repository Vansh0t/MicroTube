namespace MicroTube.Data.Models
{
	public class Video
	{
		public Guid Id { get; set; }
		public required string Title { get; set; }
		public string? Description { get; set; }
		public Guid UploaderId { get; set; }
		public AppUser? Uploader { get; set; }
		public required string Url { get; set; }
		public string? ThumbnailUrls { get; set; }
		public string? SnapshotUrls { get; set; }
		public required DateTime UploadTime { get; set; }
		public required int LengthSeconds { get; set; }
	}
}
