using MicroTube.Data.Models;

namespace MicroTube.Services.Search
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
