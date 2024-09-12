using MicroTube.Data.Models;
using MicroTube.Services.VideoContent.Reactions;

namespace MicroTube.Tests.Unit.VideoContent.Reactions
{
	public class DefaultVideoReactionsAggregatorTests
	{
		[Fact]
		public void UpdateReactionsAggregation_Success()
		{
			var aggregator = new DefaultVideoReactionsAggregator();
			VideoReactionsAggregation aggregation = new() { Dislikes =99, Likes = 99 };
			aggregation = aggregator.UpdateReactionsAggregation(aggregation, ReactionType.Like, ReactionType.None);
			Assert.Equal(100, aggregation.Likes);
			Assert.Equal(99, aggregation.Dislikes);
			aggregation = aggregator.UpdateReactionsAggregation(aggregation, ReactionType.Dislike, ReactionType.Like);
			Assert.Equal(99, aggregation.Likes);
			Assert.Equal(100, aggregation.Dislikes);
			aggregation = aggregator.UpdateReactionsAggregation(aggregation, ReactionType.Like, ReactionType.Dislike);
			Assert.Equal(100, aggregation.Likes);
			Assert.Equal(99, aggregation.Dislikes);
			aggregation = aggregator.UpdateReactionsAggregation(aggregation, ReactionType.None, ReactionType.Like);
			Assert.Equal(99, aggregation.Likes);
			Assert.Equal(99, aggregation.Dislikes);
			aggregation = aggregator.UpdateReactionsAggregation(aggregation, ReactionType.None, ReactionType.None);
			Assert.Equal(99, aggregation.Likes);
			Assert.Equal(99, aggregation.Dislikes);
			aggregation = aggregator.UpdateReactionsAggregation(aggregation, ReactionType.Dislike, ReactionType.None);
			Assert.Equal(99, aggregation.Likes);
			Assert.Equal(100, aggregation.Dislikes);
			aggregation = aggregator.UpdateReactionsAggregation(aggregation, ReactionType.None, ReactionType.Dislike);
			Assert.Equal(99, aggregation.Likes);
			Assert.Equal(99, aggregation.Dislikes);
		}
	}
}
