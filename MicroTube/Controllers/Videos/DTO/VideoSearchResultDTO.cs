namespace MicroTube.Controllers.Videos.DTO
{
	public class VideoSearchResultDTO
	{
		public IEnumerable<VideoDTO> Videos { get; set; }
		public string? Meta { get; set; }
		public VideoSearchResultDTO(IEnumerable<VideoDTO> videos)
		{
			Videos = videos;
		}
	}
}
