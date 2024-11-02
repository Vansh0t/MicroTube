using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using MicroTube.Services.Reactions;

namespace MicroTube.Data.Models.Comments
{
    [Index(nameof(UserId), nameof(CommentId), IsUnique = true)]
	public class VideoCommentReaction
	{
		[Key]
		public Guid Id { get; set; }
		[Required]
		[ForeignKey(nameof(User))]
		public Guid UserId { get; set; }
		public AppUser? User { get; set; }
		[Required]
		[ForeignKey(nameof(Comment))]
		public Guid CommentId { get; set; }
		public VideoComment? Comment { get; set; }
		public required LikeDislikeReactionType ReactionType { get; set; }
		public required DateTime Time { get; set; }
	}
}
