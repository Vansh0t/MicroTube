namespace MicroTube.Controllers.Comments.DTO
{
	public class CommentRequestDTO
	{
		public string Content { get; set; }
		public CommentRequestDTO(string content)
		{
			Content = content;
		}
	}
}
