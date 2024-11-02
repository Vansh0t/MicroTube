using MicroTube.Data.Models;

namespace MicroTube.Services.Reactions
{
    public class LikeDislikeReactionAggregator : ILikeDislikeReactionAggregator
    {
        public ILikeDislikeReactionsAggregation UpdateReactionsAggregation(ILikeDislikeReactionsAggregation aggregation, LikeDislikeReactionType newReaction, LikeDislikeReactionType previousReaction)
        {
            if (previousReaction == newReaction)
                return aggregation;
            switch (previousReaction)
            {
                case LikeDislikeReactionType.None:
                    switch (newReaction)
                    {
                        case LikeDislikeReactionType.Like:
                            return HandleNeutralToLikeAggregation(aggregation);
                        case LikeDislikeReactionType.Dislike:
                            return HandleNeutralToDislikeAggregation(aggregation);
                    }
                    break;
                case LikeDislikeReactionType.Like:
                    switch (newReaction)
                    {
                        case LikeDislikeReactionType.None:
                            return HandleLikeToNeutralAggregation(aggregation);
                        case LikeDislikeReactionType.Dislike:
                            return HandleLikeToDislikeAggregation(aggregation);
                    }
                    break;
                case LikeDislikeReactionType.Dislike:
                    switch (newReaction)
                    {
                        case LikeDislikeReactionType.None:
                            return HandleDislikeToNeutralAggregation(aggregation);
                        case LikeDislikeReactionType.Like:
                            return HandleDislikeToLikeAggregation(aggregation);
                    }
                    break;
            }
            return aggregation;
        }
        private ILikeDislikeReactionsAggregation HandleLikeToDislikeAggregation(ILikeDislikeReactionsAggregation aggregation)
        {
            aggregation.Likes--;
            aggregation.Dislikes++;
            return aggregation;
        }
        private ILikeDislikeReactionsAggregation HandleDislikeToLikeAggregation(ILikeDislikeReactionsAggregation aggregation)
        {
            aggregation.Likes++;
            aggregation.Dislikes--;
            return aggregation;
        }
        private ILikeDislikeReactionsAggregation HandleLikeToNeutralAggregation(ILikeDislikeReactionsAggregation aggregation)
        {
            aggregation.Likes--;
            return aggregation;
        }
        private ILikeDislikeReactionsAggregation HandleDislikeToNeutralAggregation(ILikeDislikeReactionsAggregation aggregation)
        {
            aggregation.Dislikes--;
            return aggregation;
        }
        private ILikeDislikeReactionsAggregation HandleNeutralToLikeAggregation(ILikeDislikeReactionsAggregation aggregation)
        {
            aggregation.Likes++;
            return aggregation;
        }
        private ILikeDislikeReactionsAggregation HandleNeutralToDislikeAggregation(ILikeDislikeReactionsAggregation aggregation)
        {
            aggregation.Dislikes++;
            return aggregation;
        }
    }
}
