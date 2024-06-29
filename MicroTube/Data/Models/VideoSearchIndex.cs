using System.Runtime.CompilerServices;

namespace MicroTube.Data.Models
{
    public class VideoSearchIndex
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
		public DateTime UploadedAt { get; set; }
        public int SearchHits { get; set; }
        public VideoSearchIndex(string id, string title, string? description)
        {
            Id = id;
            Title = title;
            Description = description != null ? description : "";
        }
    }
}
