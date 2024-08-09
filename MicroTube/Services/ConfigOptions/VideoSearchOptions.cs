namespace MicroTube.Services.ConfigOptions
{
	public class VideoSearchOptions
	{
		public const string KEY = "VideoSearch";

		public int MaxSuggestions { get; set; }
		public string VideosIndexName { get; set; }
		public string SuggestionsIndexName { get; set; }
		public int ShortVideoSeconds { get; set; }
		public int MediumVideoSeconds { get; set; }
		public int LongVideoSeconds { get; set; }
		public int PaginationBatchSize { get; set; }
		public VideoSearchOptions(int maxSuggestions,
							string videosIndexName,
							string suggestionsIndexName,
							int shortVideoSeconds,
							int mediumVideoSeconds,
							int longVideoSeconds,
							int paginationBatchSize)
		{
			MaxSuggestions = maxSuggestions;
			VideosIndexName = videosIndexName;
			SuggestionsIndexName = suggestionsIndexName;
			ShortVideoSeconds = shortVideoSeconds;
			MediumVideoSeconds = mediumVideoSeconds;
			LongVideoSeconds = longVideoSeconds;
			PaginationBatchSize = paginationBatchSize;
		}
	}
}
