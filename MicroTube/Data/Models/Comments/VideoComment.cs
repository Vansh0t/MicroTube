using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MicroTube.Data.Models.Reactions;

namespace MicroTube.Data.Models.Comments
{
	public class VideoComment : IComment, IReactable
	{
		public Guid Id { get; set; }
		[Column(TypeName = "NVARCHAR"), StringLength(512)]
		public required string Content { get; set; }
		[Required]
		[ForeignKey(nameof(User))]
		public Guid UserId { get; set; }
		public AppUser? User { get; set; }
		[Required]
		[ForeignKey(nameof(Video))]
		public Guid VideoId { get; set; }
		public Video? Video { get; set; }
		public bool Edited { get; set; }
		public bool Deleted { get; set; }
		public required DateTime Time {get;set;}
		public VideoCommentReactionsAggregation? Reactions { get; set; }

		[NotMapped]
		public Guid TargetId { get => VideoId; set => VideoId = value; }
		[NotMapped]
		public IReactionsAggregation? ReactionsAggregation { get => Reactions;}
	}
}
