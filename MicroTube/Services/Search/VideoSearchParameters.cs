namespace MicroTube.Services.Search
{
	public class VideoSearchParameters
	{

		public string? Text { get; set; }
		public VideoSortType SortType { get; set; } = VideoSortType.Relevance;
		public VideoTimeFilterType TimeFilter { get; set; } = VideoTimeFilterType.None;
		public VideoLengthFilterType LengthFilter { get; set; } = VideoLengthFilterType.None;
		public string? Meta { get; set; }
	}
}
