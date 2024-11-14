using MicroTube.Data.Access;

namespace MicroTube.Services.Reactions
{
    public class LikeDislikeReactionAggregationHandler : ILikeDislikeReactionAggregationHandler
	{
		public LikeDislikeAggregationResult GetAggregationChange(LikeDislikeReactionType newReaction, LikeDislikeReactionType previousReaction)
        {
			var result = new LikeDislikeAggregationResult();
            if (previousReaction == newReaction)
                return result;
            switch (previousReaction)
            {
                case LikeDislikeReactionType.None:
                    switch (newReaction)
                    {
                        case LikeDislikeReactionType.Like:
                            return HandleNeutralToLikeAggregation(result);
                        case LikeDislikeReactionType.Dislike:
                            return HandleNeutralToDislikeAggregation(result);
                    }
                    break;
                case LikeDislikeReactionType.Like:
                    switch (newReaction)
                    {
                        case LikeDislikeReactionType.None:
                            return HandleLikeToNeutralAggregation(result);
                        case LikeDislikeReactionType.Dislike:
                            return HandleLikeToDislikeAggregation(result);
                    }
                    break;
                case LikeDislikeReactionType.Dislike:
                    switch (newReaction)
                    {
                        case LikeDislikeReactionType.None:
                            return HandleDislikeToNeutralAggregation(result);
                        case LikeDislikeReactionType.Like:
                            return HandleDislikeToLikeAggregation(result);
                    }
                    break;
            }
            return result;
        }
        private LikeDislikeAggregationResult HandleLikeToDislikeAggregation(LikeDislikeAggregationResult aggregation)
        {
            aggregation.LikesChange--;
            aggregation.DislikesChange++;
            return aggregation;
        }
        private LikeDislikeAggregationResult HandleDislikeToLikeAggregation(LikeDislikeAggregationResult aggregation)
        {
            aggregation.LikesChange++;
            aggregation.DislikesChange--;
            return aggregation;
        }
        private LikeDislikeAggregationResult HandleLikeToNeutralAggregation(LikeDislikeAggregationResult aggregation)
        {
            aggregation.LikesChange--;
            return aggregation;
        }
        private LikeDislikeAggregationResult HandleDislikeToNeutralAggregation(LikeDislikeAggregationResult aggregation)
        {
            aggregation.DislikesChange--;
            return aggregation;
        }
        private LikeDislikeAggregationResult HandleNeutralToLikeAggregation(LikeDislikeAggregationResult aggregation)
        {
            aggregation.LikesChange++;
            return aggregation;
        }
        private LikeDislikeAggregationResult HandleNeutralToDislikeAggregation(LikeDislikeAggregationResult aggregation)
        {
            aggregation.DislikesChange++;
            return aggregation;
        }
    }
}
