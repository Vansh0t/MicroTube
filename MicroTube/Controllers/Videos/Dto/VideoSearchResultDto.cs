namespace MicroTube.Controllers.Videos.Dto
{
	public class VideoSearchResultDto
	{
		public IEnumerable<VideoDto> Videos { get; set; }
		public string? Meta { get; set; }
		public VideoSearchResultDto(IEnumerable<VideoDto> videos)
		{
			Videos = videos;
		}
	}
}
