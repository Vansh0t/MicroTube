using MicroTube.Data.Models;
using MicroTube.Services.Reactions;

namespace MicroTube.Services.VideoContent.Likes
{
	public interface IVideoReactionsService
	{
		Task<IServiceResult<UserVideoReaction>> GetReaction(string userId, string videoId);
		Task<IServiceResult<UserVideoReaction>> SetReaction(string userId, string videoId, LikeDislikeReactionType reactionType);
	}
}