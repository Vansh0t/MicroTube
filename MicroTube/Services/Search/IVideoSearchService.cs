using MicroTube.Data.Models;

namespace MicroTube.Services.Search
{
    public interface IVideoSearchService
	{
		Task<IServiceResult<Video>> IndexVideo(Video video);
		Task<IServiceResult<IReadOnlyCollection<VideoSearchIndex>>> GetVideos(string text);
		Task<IServiceResult> IndexSearchSuggestion(string text);
		Task<IServiceResult<IReadOnlyCollection<VideoSearchSuggestionIndex>>> GetSuggestions(string input);
	}
}