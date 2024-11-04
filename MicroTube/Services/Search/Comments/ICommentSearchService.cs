
namespace MicroTube.Services.Search.Comments
{
	public interface ICommentSearchService
	{
		Task<IServiceResult<VideoCommentSearchResult>> GetComments(string videoId, VideoCommentSearchParameters parameters, string? meta);
	}
}