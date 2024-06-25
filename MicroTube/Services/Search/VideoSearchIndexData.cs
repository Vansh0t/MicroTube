using System.Runtime.CompilerServices;

namespace MicroTube.Services.Search
{
	public class VideoSearchIndexData
	{
		public string Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public int SearchHits { get; set; }
		public VideoSearchIndexData(string id, string title, string? description)
		{
			Id = id;
			Title = title;
			Description = description != null ? description: "";
		}
	}
}
