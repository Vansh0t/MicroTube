namespace MicroTube.Controllers.Comments.Dto
{
	public class CommentSearchResultDto
	{

		public IEnumerable<CommentDto> Comments { get; set; }
		public string? Meta { get; set; }
		public CommentSearchResultDto(IEnumerable<CommentDto> comments, string? meta)
		{
			Comments = comments;
			Meta = meta;
		}
	}
}
