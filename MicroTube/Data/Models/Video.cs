namespace MicroTube.Data.Models
{
	public class Video
	{
		public Guid Id { get; set; }
		public required string Title { get; set; }
		public string? Description { get; set; }
		public required Guid UploaderId { get; set; }
		public AppUser? Uploader { get; set; }
		public required string Urls { get; set; }
		public required string ThumbnailUrls { get; set; }
		public DateTime UploadTime { get; set; }
		public int LengthSeconds { get; set; }
		public string? SearchIndexId { get; set; }
	}
}
