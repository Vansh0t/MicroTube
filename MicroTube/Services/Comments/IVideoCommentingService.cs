using MicroTube.Data.Models.Comments;
using MicroTube.Services.Reactions;

namespace MicroTube.Services.Comments
{
	public interface IVideoCommentingService
	{
		Task<IServiceResult<VideoComment>> Comment(string userId, string videoId, string content);
		Task<IServiceResult<VideoComment>> DeleteComment(string userId, string commentId);
		Task<IServiceResult<VideoComment>> EditComment(string userId, string newContent, string commentId);
		Task<IServiceResult<VideoCommentReaction>> ReactToComment(string userId, string commentId, LikeDislikeReactionType reactionType);
	}
}