﻿using MicroTube.Services.Search;

namespace MicroTube.Controllers.Videos.DTO
{
	public class VideoSearchParametersDTO
	{
		public required string Text { get; set; }
		public VideoSortType Sort { get; set; } = VideoSortType.Relevance;
		public VideoTimeFilterType TimeFilter { get; set; } = VideoTimeFilterType.None;
		public VideoLengthFilterType LengthFilter { get; set; } = VideoLengthFilterType.None;
	}
}