namespace MicroTube.Controllers.Videos.DTO
{
	public class VideoSearchSuggestion
	{
		public string Id { get; set; }
		public string Title { get; set; }
		public VideoSearchSuggestion(string id, string title)
		{
			Id = id;
			Title = title;
		}
	}
}
