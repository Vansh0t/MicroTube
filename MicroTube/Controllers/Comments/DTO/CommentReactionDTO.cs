using MicroTube.Services.Reactions;

namespace MicroTube.Controllers.Comments.DTO
{
	public class CommentReactionDTO
	{

		public string UserId { get; set; }
		public string CommentId { get; set; }
		public DateTime Time { get; set; }
		public LikeDislikeReactionType ReactionType { get; set; }
		public CommentReactionDTO(string userId, string commentId, DateTime time, LikeDislikeReactionType reactionType)
		{
			UserId = userId;
			CommentId = commentId;
			Time = time;
			ReactionType = reactionType;
		}
	}
}
