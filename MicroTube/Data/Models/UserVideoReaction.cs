using Microsoft.EntityFrameworkCore;
using MicroTube.Services.Reactions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroTube.Data.Models
{
    [Index(nameof(UserId), nameof(VideoId), IsUnique =true)]
	public class UserVideoReaction
	{
		[Key]
		public Guid Id { get; set; }
		[Required]
		[ForeignKey(nameof(User))]
		public Guid UserId { get; set; }
		public AppUser? User { get; set; }
		[Required]
		[ForeignKey(nameof(Video))]
		public Guid VideoId { get; set; }
		public Video? Video { get; set; }
		public required LikeDislikeReactionType ReactionType { get; set; }
		public required DateTime Time { get; set; }
	}
}
