namespace MicroTube.Data.Models
{
	public class VideoReactions
	{
		public required Guid VideoId { get; set; }
		public Video? Video { get; set; }
		public required int Likes { get; set; }
		public required int Dislikes { get; set; }
	}
}
