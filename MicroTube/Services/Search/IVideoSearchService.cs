using MicroTube.Data.Models;

namespace MicroTube.Services.Search
{
	public interface IVideoSearchService
	{
		Task<IServiceResult<IReadOnlyCollection<VideoSearchIndexData>>> GetSuggestions(string input);
		Task<IServiceResult<Video>> IndexVideo(Video video);
	}
}