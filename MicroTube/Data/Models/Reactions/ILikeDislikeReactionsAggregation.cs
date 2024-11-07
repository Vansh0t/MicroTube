namespace MicroTube.Data.Models.Reactions
{
	public interface ILikeDislikeReactionsAggregation : IReactionsAggregation
	{
		int Likes { get; set; }
		int Dislikes { get; set; }
		int Difference { get; set; }
	}
}
