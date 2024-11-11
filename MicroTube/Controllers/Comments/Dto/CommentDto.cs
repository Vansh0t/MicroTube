using MicroTube.Controllers.Reactions.Dto;

namespace MicroTube.Controllers.Comments.Dto
{
	public class CommentDto
	{
		public string Id { get; set; }
		public string? UserId { get; set; }
		public string? UserAlias { get; set; }
		public string? TargetId { get; set; }
		public string Content { get; set; }
		public DateTime Time { get; set; }
		public bool Edited { get; set; }
		public LikeDislikeReactionDto? Reaction { get; set; }
		public LikeDislikeReactionsAggregationDto? ReactionsAggregation {get;set;}
		public CommentDto(string id, string content, DateTime time, bool edited)
		{
			Id = id;
			Content = content;
			Time = time;
			Edited = edited;
		}
	}
}
