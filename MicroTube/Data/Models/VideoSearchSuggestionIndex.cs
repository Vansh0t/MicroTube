namespace MicroTube.Data.Models
{
    public class VideoSearchSuggestionIndex
    {
		public string Id { get; set; }
        public string Text { get; set; }
        public int SearchHits { get; set; }
		public VideoSearchSuggestionIndex(string id, string text, int searchHits)
		{
			Text = text;
			SearchHits = searchHits;
			Id = id;
		}
	}
}
