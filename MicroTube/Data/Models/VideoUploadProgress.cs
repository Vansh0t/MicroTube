using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MicroTube.Data.Models
{
	public class VideoUploadProgress
	{
		[Key]
		public Guid Id { get; set; }
		public required VideoUploadStatus Status { get; set; }
		[Required]
		[ForeignKey(nameof(Uploader))]
		public Guid UploaderId { get; set; }
		public AppUser? Uploader { get; set; }
		[Column(TypeName = "NVARCHAR"), StringLength(200)]
		public required string Title { get; set; }
		[Column(TypeName = "NVARCHAR"), StringLength(1000)]
		public string? Description { get; set; }
		[Column(TypeName = "VARCHAR"), StringLength(50)]
		public required string SourceFileRemoteCacheLocation { get; set; }
		[Column(TypeName = "VARCHAR"), StringLength(50)]
		public required string SourceFileRemoteCacheFileName { get; set; }
		public required DateTime Timestamp { get; set; }
		[Column(TypeName = "NVARCHAR"), StringLength(200)]
		public string? Message { get; set; }
		public int? LengthSeconds { get; set; }
		[Column(TypeName = "VARCHAR"), StringLength(36)]
		public string? FrameSize { get; set; }
		[Column(TypeName = "VARCHAR"), StringLength(20)]
		public string? Format { get; set; }
		public int? Fps { get; set; }

	}
	public enum VideoUploadStatus {InQueue, InProgress, Fail, Success}
}
