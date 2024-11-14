using MicroTube.Data.Models.Reactions;

namespace MicroTube.Services.Reactions
{
    public interface ILikeDislikeReactionAggregationHandler
	{
		LikeDislikeAggregationResult GetAggregationChange(LikeDislikeReactionType newReaction, LikeDislikeReactionType previousReaction);
	}
}