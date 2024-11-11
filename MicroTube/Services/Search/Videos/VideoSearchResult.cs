using MicroTube.Data.Models.Videos;

namespace MicroTube.Services.Search.Videos
{
    public class VideoSearchResult
	{
		public IEnumerable<VideoSearchIndex> Indices { get; set; }
		public string? Meta { get; set; }
		public VideoSearchResult(IEnumerable<VideoSearchIndex> indices)
		{
			Indices = indices;
		}

	}
}
