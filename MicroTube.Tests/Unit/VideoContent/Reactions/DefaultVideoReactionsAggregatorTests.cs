using MicroTube.Data.Models;
using MicroTube.Data.Models.Reactions;
using MicroTube.Services.Reactions;

namespace MicroTube.Tests.Unit.VideoContent.Reactions
{
	public class DefaultVideoReactionsAggregatorTests
	{
		[Fact]
		public void UpdateReactionsAggregation_Success()
		{
			var aggregator = new LikeDislikeReactionAggregator();
			ILikeDislikeReactionsAggregation aggregation = new VideoReactionsAggregation { Dislikes =99, Likes = 99 };
			aggregation = aggregator.UpdateReactionsAggregation(aggregation, LikeDislikeReactionType.Like, LikeDislikeReactionType.None);
			Assert.Equal(100, aggregation.Likes);
			Assert.Equal(99, aggregation.Dislikes);
			aggregation = aggregator.UpdateReactionsAggregation(aggregation, LikeDislikeReactionType.Dislike, LikeDislikeReactionType.Like);
			Assert.Equal(99, aggregation.Likes);
			Assert.Equal(100, aggregation.Dislikes);
			aggregation = aggregator.UpdateReactionsAggregation(aggregation, LikeDislikeReactionType.Like, LikeDislikeReactionType.Dislike);
			Assert.Equal(100, aggregation.Likes);
			Assert.Equal(99, aggregation.Dislikes);
			aggregation = aggregator.UpdateReactionsAggregation(aggregation, LikeDislikeReactionType.None, LikeDislikeReactionType.Like);
			Assert.Equal(99, aggregation.Likes);
			Assert.Equal(99, aggregation.Dislikes);
			aggregation = aggregator.UpdateReactionsAggregation(aggregation, LikeDislikeReactionType.None, LikeDislikeReactionType.None);
			Assert.Equal(99, aggregation.Likes);
			Assert.Equal(99, aggregation.Dislikes);
			aggregation = aggregator.UpdateReactionsAggregation(aggregation, LikeDislikeReactionType.Dislike, LikeDislikeReactionType.None);
			Assert.Equal(99, aggregation.Likes);
			Assert.Equal(100, aggregation.Dislikes);
			aggregation = aggregator.UpdateReactionsAggregation(aggregation, LikeDislikeReactionType.None, LikeDislikeReactionType.Dislike);
			Assert.Equal(99, aggregation.Likes);
			Assert.Equal(99, aggregation.Dislikes);
		}
	}
}
