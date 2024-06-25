namespace MicroTube.Services.ConfigOptions
{
	public class VideoSearchOptions
	{
		public const string KEY = "VideoSearch";

		public int MaxSuggestions { get; set; }

		public VideoSearchOptions(int maxSuggestions)
		{
			MaxSuggestions = maxSuggestions;
		}
	}
}
