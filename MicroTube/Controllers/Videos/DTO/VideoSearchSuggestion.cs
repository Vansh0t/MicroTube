namespace MicroTube.Controllers.Videos.DTO
{
	public class VideoSearchSuggestion
	{
		public string Text { get; set; }
		public VideoSearchSuggestion(string text)
		{
			Text = text;
		}
	}
}
