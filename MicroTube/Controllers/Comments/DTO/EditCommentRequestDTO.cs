namespace MicroTube.Controllers.Comments.DTO
{
	public class EditCommentRequestDTO
	{
		public string NewContent { get; set; }
		public EditCommentRequestDTO(string newContent)
		{
			NewContent = newContent;
		}
	}
}
