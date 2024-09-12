using MicroTube.Data.Models;

namespace MicroTube.Services.VideoContent.Likes
{
	public interface IVideoReactionsService
	{
		Task<IServiceResult<UserVideoReaction>> GetReaction(string userId, string videoId);
		Task<IServiceResult<UserVideoReaction>> SetReaction(string userId, string videoId, ReactionType reactionType);
	}
}