using MicroTube.Services.Reactions;

namespace MicroTube.Tests.Unit.VideoContent.Reactions
{
	public class DefaultVideoReactionsAggregatorTests
	{
		[Fact]
		public void UpdateReactionsAggregation_Success()
		{
			var aggregator = new LikeDislikeReactionAggregationHandler();
			var result = aggregator.GetAggregationChange(LikeDislikeReactionType.Like, LikeDislikeReactionType.None);
			Assert.Equal(1, result.LikesChange);
			Assert.Equal(0, result.DislikesChange);
			result = aggregator.GetAggregationChange(LikeDislikeReactionType.Dislike, LikeDislikeReactionType.Like);
			Assert.Equal(-1, result.LikesChange);
			Assert.Equal(1, result.DislikesChange);
			result = aggregator.GetAggregationChange(LikeDislikeReactionType.Like, LikeDislikeReactionType.Dislike);
			Assert.Equal(1, result.LikesChange);
			Assert.Equal(-1, result.DislikesChange);
			result = aggregator.GetAggregationChange(LikeDislikeReactionType.None, LikeDislikeReactionType.Like);
			Assert.Equal(-1, result.LikesChange);
			Assert.Equal(0, result.DislikesChange);
			result = aggregator.GetAggregationChange(LikeDislikeReactionType.None, LikeDislikeReactionType.None);
			Assert.Equal(0, result.LikesChange);
			Assert.Equal(0, result.DislikesChange);
			result = aggregator.GetAggregationChange(LikeDislikeReactionType.Dislike, LikeDislikeReactionType.None);
			Assert.Equal(0, result.LikesChange);
			Assert.Equal(1, result.DislikesChange);
			result = aggregator.GetAggregationChange(LikeDislikeReactionType.None, LikeDislikeReactionType.Dislike);
			Assert.Equal(0, result.LikesChange);
			Assert.Equal(-1, result.DislikesChange);
		}
	}
}
