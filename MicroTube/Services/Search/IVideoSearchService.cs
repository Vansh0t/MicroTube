using MicroTube.Data.Models;

namespace MicroTube.Services.Search
{
    public interface IVideoSearchService
	{
		Task<IServiceResult<Video>> IndexVideo(Video video);
		Task<IServiceResult<VideoSearchResult>> GetVideos(VideoSearchParameters parameters, string? meta);
		Task<IServiceResult<IEnumerable<string>>> GetSuggestions(string input);
	}
}