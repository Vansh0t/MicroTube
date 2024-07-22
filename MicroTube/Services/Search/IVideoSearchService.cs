using MicroTube.Data.Models;

namespace MicroTube.Services.Search
{
    public interface IVideoSearchService
	{
		Task<IServiceResult<Video>> IndexVideo(Video video);
		Task<IServiceResult<IReadOnlyCollection<VideoSearchIndex>>> GetVideos(
			string text,
			VideoSortType sortType = VideoSortType.Default,
			VideoTimeFilterType timeFilter = VideoTimeFilterType.None,
			VideoLengthFilterType lengthFilter = VideoLengthFilterType.None);
		Task<IServiceResult<IReadOnlyCollection<string>>> GetSuggestions(string input);
	}
}