using MicroTube.Data.Models;

namespace MicroTube.Services.VideoContent.Reactions
{
	public class DefaultVideoReactionsAggregator : IVideoReactionsAggregator
	{
		public VideoReactionsAggregation UpdateReactionsAggregation(VideoReactionsAggregation aggregation, ReactionType newReaction, ReactionType previousReaction)
		{
			if (previousReaction == newReaction)
				return aggregation;
			switch (previousReaction)
			{
				case ReactionType.None:
					switch (newReaction)
					{
						case ReactionType.Like:
							return HandleNeutralToLikeAggregation(aggregation);
						case ReactionType.Dislike:
							return HandleNeutralToDislikeAggregation(aggregation);
					}
					break;
				case ReactionType.Like:
					switch (newReaction)
					{
						case ReactionType.None:
							return HandleLikeToNeutralAggregation(aggregation);
						case ReactionType.Dislike:
							return HandleLikeToDislikeAggregation(aggregation);
					}
					break;
				case ReactionType.Dislike:
					switch (newReaction)
					{
						case ReactionType.None:
							return HandleDislikeToNeutralAggregation(aggregation);
						case ReactionType.Like:
							return HandleDislikeToLikeAggregation(aggregation);
					}
					break;
			}
			return aggregation;
		}
		private VideoReactionsAggregation HandleLikeToDislikeAggregation(VideoReactionsAggregation aggregation)
		{
			aggregation.Likes--;
			aggregation.Dislikes++;
			return aggregation;
		}
		private VideoReactionsAggregation HandleDislikeToLikeAggregation(VideoReactionsAggregation aggregation)
		{
			aggregation.Likes++;
			aggregation.Dislikes--;
			return aggregation;
		}
		private VideoReactionsAggregation HandleLikeToNeutralAggregation(VideoReactionsAggregation aggregation)
		{
			aggregation.Likes--;
			return aggregation;
		}
		private VideoReactionsAggregation HandleDislikeToNeutralAggregation(VideoReactionsAggregation aggregation)
		{
			aggregation.Dislikes--;
			return aggregation;
		}
		private VideoReactionsAggregation HandleNeutralToLikeAggregation(VideoReactionsAggregation aggregation)
		{
			aggregation.Likes++;
			return aggregation;
		}
		private VideoReactionsAggregation HandleNeutralToDislikeAggregation(VideoReactionsAggregation aggregation)
		{
			aggregation.Dislikes++;
			return aggregation;
		}
	}
}
