namespace MicroTube.Data.Models
{
	public class Video
	{
		public Guid Id { get; set; }
		public required string Title { get; set; }
		public Guid UploaderId { get; set; }
		public AppUser? Uploader { get; set; }
		public Guid FileMetaId { get; set; }
		public FileMeta? FileMeta { get; set; }
	}
}
