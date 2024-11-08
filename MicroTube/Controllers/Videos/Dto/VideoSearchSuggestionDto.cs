namespace MicroTube.Controllers.Videos.Dto
{
	public class VideoSearchSuggestionDto
	{
		public string Text { get; set; }
		public VideoSearchSuggestionDto(string text)
		{
			Text = text;
		}
	}
}
