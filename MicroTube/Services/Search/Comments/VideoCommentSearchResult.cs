using MicroTube.Data.Models.Comments;

namespace MicroTube.Services.Search.Comments
{
	public class VideoCommentSearchResult
	{
		public string? Meta { get; set; }
		public IEnumerable<VideoComment> Comments { get; set; }
	}
}
