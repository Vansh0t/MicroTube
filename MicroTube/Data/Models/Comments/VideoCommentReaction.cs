using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using MicroTube.Services.Reactions;
using MicroTube.Data.Models.Reactions;

namespace MicroTube.Data.Models.Comments
{
    [Index(nameof(UserId), nameof(CommentId), IsUnique = true)]
	public class VideoCommentReaction : ILikeDislikeReaction
	{
		[Key]
		public Guid Id { get; set; }
		[Required]
		[ForeignKey(nameof(User))]
		public Guid UserId { get; set; }
		public AppUser? User { get; set; }
		[Required()]
		[ForeignKey(nameof(Comment))]
		public Guid CommentId { get; set; }
		[DeleteBehavior(DeleteBehavior.Restrict)] //TO DO: Removing this causes cascading/cycle FK error and I have no idea why
		public VideoComment? Comment { get; set; }
		public required LikeDislikeReactionType ReactionType { get; set; }
		public required DateTime Time { get; set; }
		
		[NotMapped]
		public Guid TargetId { get => CommentId; set => CommentId = value; }
		[NotMapped]
		public IReactable? Target { get => Comment; }
	}
}
