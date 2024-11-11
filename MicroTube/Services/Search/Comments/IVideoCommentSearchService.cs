
namespace MicroTube.Services.Search.Comments
{
	public interface IVideoCommentSearchService
	{
		Task<IServiceResult<VideoCommentSearchResult>> GetComments(string targetId, VideoCommentSearchParameters parameters, string? meta);
	}
}