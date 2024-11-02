using MicroTube.Data.Models;

namespace MicroTube.Services.Reactions
{
    public interface ILikeDislikeReactionAggregator
	{
		ILikeDislikeReactionsAggregation UpdateReactionsAggregation(ILikeDislikeReactionsAggregation aggregation, LikeDislikeReactionType newReaction, LikeDislikeReactionType previousReaction);
	}
}