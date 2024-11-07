using MicroTube.Data.Models;
using MicroTube.Services.Reactions;

namespace MicroTube.Services.VideoContent.Likes
{
	public interface IVideoReactionsService
	{
		Task<IServiceResult<VideoReaction>> GetReaction(string userId, string videoId);
		Task<IServiceResult<VideoReaction>> SetReaction(string userId, string videoId, LikeDislikeReactionType reactionType);
	}
}