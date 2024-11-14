namespace MicroTube.Controllers.Comments.Dto
{
	public class EditCommentRequestDto
	{
		public string NewContent { get; set; }
		public EditCommentRequestDto(string newContent)
		{
			NewContent = newContent;
		}
	}
}
