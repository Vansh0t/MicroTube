using MicroTube.Data.Models.Videos;

namespace MicroTube.Services.Search.Videos
{
    public interface IVideoSearchService
	{
		Task<IServiceResult<Video>> IndexVideo(Video video);
		Task<IServiceResult<VideoSearchResult>> GetVideos(VideoSearchParameters parameters, string? meta);
		Task<IServiceResult<IEnumerable<string>>> GetSuggestions(string input);
	}
}