namespace MicroTube.Controllers.Comments.DTO
{
	public class CommentSearchResultDTO
	{

		public IEnumerable<CommentDTO> Comments { get; set; }
		public string? Meta { get; set; }
		public CommentSearchResultDTO(IEnumerable<CommentDTO> comments, string? meta)
		{
			Comments = comments;
			Meta = meta;
		}
	}
}
