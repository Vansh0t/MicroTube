using MicroTube.Services.Search;

namespace MicroTube.Controllers.Comments.Dto
{
	public class CommentSearchParametersDto
	{
		public VideoCommentSortType SortType { get; set; }
		public int BatchSize { get; set; }
	}
}
