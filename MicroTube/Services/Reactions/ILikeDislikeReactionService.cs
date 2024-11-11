using MicroTube.Data.Models.Reactions;

namespace MicroTube.Services.Reactions
{
    public interface ILikeDislikeReactionService
    {
		Task<IServiceResult<ILikeDislikeReaction>> SetReaction(string userId, string targetId, LikeDislikeReactionType reactionType);
		Task<IServiceResult<ILikeDislikeReaction>> GetReaction(string userId, string targetId);
    }
}
