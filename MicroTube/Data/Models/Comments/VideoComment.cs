using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MicroTube.Data.Models.Comments
{
	public class VideoComment
	{
		public Guid Id { get; set; }
		[Column(TypeName = "NVARCHAR"), StringLength(512)]
		public required string Content { get; set; }
		[Required]
		[ForeignKey(nameof(User))]
		public Guid UserId { get; set; }
		public AppUser? User { get; set; }
		public bool Edited { get; set; }
		public bool Deleted { get; set; }
		[Required]
		[ForeignKey(nameof(Video))]
		public Guid VideoId { get; set; }
		public required DateTime UploadTime {get;set;}
		public Video? Video { get; set; }
		public VideoCommentReactionsAggregation? Reactions { get; set; }
	}
}
