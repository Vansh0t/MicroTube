namespace MicroTube.Services.Search
{
	public class VideoSearchParameters
	{

		public string? Text { get; set; }
		public VideoSortType SortType { get; set; }
		public VideoTimeFilterType TimeFilter { get; set; } = VideoTimeFilterType.None;
		public VideoLengthFilterType LengthFilter { get; set; } = VideoLengthFilterType.None;
		public int BatchSize { get; set; }
		public string? UploaderId { get; set; }
	}
}
