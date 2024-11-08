namespace MicroTube.Controllers.Reactions.Dto
{
	public class LikeDislikeReactionsAggregationDto
	{
		public string TargetId { get; set; }
		public int Likes { get; set; }
		public int Dislikes { get; set; }
		public int Difference { get; set; }
		public LikeDislikeReactionsAggregationDto(string targetId, int likes, int dislikes, int difference)
		{
			TargetId = targetId;
			Likes = likes;
			Dislikes = dislikes;
			Difference = difference;
		}
	}
}
