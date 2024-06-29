namespace MicroTube.Services.ConfigOptions
{
	public class VideoSearchOptions
	{
		public const string KEY = "VideoSearch";

		public int MaxSuggestions { get; set; }
		public string VideosIndexName { get; set; }
		public string SuggestionsIndexName { get; set; }
		public VideoSearchOptions(int maxSuggestions, string videosIndexName, string suggestionsIndexName)
		{
			MaxSuggestions = maxSuggestions;
			VideosIndexName = videosIndexName;
			SuggestionsIndexName = suggestionsIndexName;
		}
	}
}
