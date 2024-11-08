namespace MicroTube.Controllers.Comments.Dto
{
	public class CommentRequestDto
	{
		public string Content { get; set; }
		public CommentRequestDto(string content)
		{
			Content = content;
		}
	}
}
