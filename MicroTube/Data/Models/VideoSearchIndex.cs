using System.Runtime.CompilerServices;

namespace MicroTube.Data.Models
{
    public class VideoSearchIndex
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
		public DateTime UploadedAt { get; set; }
		public int Views { get; set; }
		public int Likes { get; set; }
		public int Dislikes { get; set; }
		public int LengthSeconds { get; set; }
		public string TitleSuggestion { get; set; }
        public int SearchHits { get; set; }
		public VideoSearchIndex(string id,
			string title,
			string? description,
			string titleSuggestion,
			int views,
			int likes,
			int dislikes,
			int lengthSeconds,
			DateTime uploadedAt)
		{
			Id = id;
			Title = title;
			Description = description != null ? description : "";
			TitleSuggestion = titleSuggestion;
			Views = views;
			Likes = likes;
			LengthSeconds = lengthSeconds;
			UploadedAt = uploadedAt;
			Dislikes = dislikes;
		}
	}
}
