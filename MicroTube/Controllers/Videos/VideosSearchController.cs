using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicroTube.Controllers.Videos.Dto;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.Search;
using MicroTube.Services.Search.Videos;

namespace MicroTube.Controllers.Videos
{
	[Route("Videos/[controller]")]
	[ApiController]
	public class VideosSearchController : ControllerBase
	{
		private readonly IVideoSearchService _searchService;
		private readonly ILogger<VideosSearchController> _logger;
		private readonly MicroTubeDbContext _db;
		public VideosSearchController(IVideoSearchService searchService, ILogger<VideosSearchController> logger, MicroTubeDbContext db)
		{
			_searchService = searchService;
			_logger = logger;
			_db = db;
		}

		[HttpGet("suggestions/{text}")]
		public async Task<IActionResult> GetSuggestions(string text)
		{
			var result = await _searchService.GetSuggestions(text);
			if (result.IsError)
				return StatusCode(result.Code);
			return Ok(result.GetRequiredObject().Select(_ => {
				return new VideoSearchSuggestionDto(
								_);
			}));
		}
		[HttpPost("videos")]
		public async Task<IActionResult> GetVideos([FromQuery] VideoSearchParametersDto searchParameters, [FromBody] VideoRequestMetaDto? meta)
		{
			var searchResult = await _searchService.GetVideos(new VideoSearchParameters()
			{
				Text = searchParameters.Text,
				SortType = searchParameters.Sort,
				TimeFilter = searchParameters.TimeFilter,
				LengthFilter = searchParameters.LengthFilter,
				BatchSize = searchParameters.BatchSize,
				UploaderId = searchParameters.UploaderIdFilter
			}, meta != null ? meta.Meta : null);
			if (searchResult.IsError)
				return StatusCode(searchResult.Code);
			var searchData = searchResult.GetRequiredObject();
			var ids = searchData.Indices.Select(_ => _.Id);
			var videosResult = await _db.Videos.Where(_ => ids.Contains(_.Id.ToString())).ToArrayAsync();
			IEnumerable<Video> videosResultSorted = searchData.Indices.Join(
				videosResult, outer => outer.Id, inner => inner.Id.ToString(), (index, result)=> result);
			var sortedVideos = videosResultSorted.Select(VideoDto.FromModel).ToArray();
			return Ok(new VideoSearchResultDto(sortedVideos) { Meta = searchData.Meta });
		}
		[HttpGet("controls")]
		public IActionResult GetControls()
		{
			var controls = new SearchControlsDto()
			{
				LengthFilterOptions = new string[3] {
					nameof(VideoLengthFilterType.Short),
					nameof(VideoLengthFilterType.Medium),
					nameof(VideoLengthFilterType.Long)
				},
				TimeFilterOptions = new string[5] {
					nameof(VideoTimeFilterType.LastDay),
					nameof(VideoTimeFilterType.LastWeek),
					nameof(VideoTimeFilterType.LastMonth),
					nameof(VideoTimeFilterType.LastSixMonths),
					nameof(VideoTimeFilterType.LastYear),
				},
				SortOptions = new string[4] {
					nameof(VideoSortType.Relevance),
					nameof(VideoSortType.Time),
					nameof(VideoSortType.Rating),
					nameof(VideoSortType.Views),
				}
			};
			return Ok(controls);
		}
	}
}
