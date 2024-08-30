using MicroTube.Data.Models;

namespace MicroTube.Services.VideoContent.Reactions
{
	public interface IVideoReactionsAggregator
	{
		VideoReactionsAggregation UpdateReactionsAggregation(VideoReactionsAggregation aggregation, ReactionType newReaction, ReactionType previousReaction);
	}
}