using MicroTube.Services.Search;

namespace MicroTube.Controllers.Videos.Dto
{
	public class VideoSearchParametersDto
	{
		public string? Text { get; set; }
		public VideoSortType Sort { get; set; } = VideoSortType.Relevance;
		public VideoTimeFilterType TimeFilter { get; set; } = VideoTimeFilterType.None;
		public VideoLengthFilterType LengthFilter { get; set; } = VideoLengthFilterType.None;
		public int BatchSize { get; set; }
		public string? UploaderIdFilter { get; set; }
	}
}
