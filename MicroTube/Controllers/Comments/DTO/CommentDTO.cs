namespace MicroTube.Controllers.Comments.DTO
{
	public class CommentDTO
	{
		public string Id { get; set; }
		public string? UserId { get; set; }
		public string? UserAlias { get; set; }
		public string? TargetId { get; set; }
		public string Content { get; set; }
		public DateTime Time { get; set; }
		public bool Edited { get; set; }
		public CommentDTO(string id, string content, DateTime time, bool edited)
		{
			Id = id;
			Content = content;
			Time = time;
			Edited = edited;
		}
	}
}
