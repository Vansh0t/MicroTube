using MicroTube.Services.Search;

namespace MicroTube.Controllers.Comments.DTO
{
	public class CommentSearchParametersDTO
	{
		public VideoCommentSortType SortType { get; set; }
		public int BatchSize { get; set; }
	}
}
