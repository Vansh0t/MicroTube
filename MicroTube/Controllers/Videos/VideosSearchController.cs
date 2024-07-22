using Hangfire;
using Microsoft.AspNetCore.Mvc;
using MicroTube.Controllers.Videos.DTO;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.Search;

namespace MicroTube.Controllers.Videos
{
    [Route("Videos/[controller]")]
	[ApiController]
	public class VideosSearchController : ControllerBase
	{
		private readonly IVideoSearchService _searchService;
		private readonly IVideoDataAccess _videoDataAccess;
		private readonly ILogger<VideosSearchController> _logger;
		public VideosSearchController(IVideoSearchService searchService, IVideoDataAccess videoDataAccess, ILogger<VideosSearchController> logger)
		{
			_searchService = searchService;
			_videoDataAccess = videoDataAccess;
			_logger = logger;
		}

		[HttpGet("suggestions/{text}")]
		public async Task<IActionResult> GetSuggestions(string text)
		{
			var result = await _searchService.GetSuggestions(text);
			if (result.IsError)
				return StatusCode(result.Code);
			return Ok(result.GetRequiredObject().Select(_ => {
				return new VideoSearchSuggestion(
								_);
			}));
		}
		[HttpGet("videos")]
		public async Task<IActionResult> GetVideos([FromQuery] VideoSearchParametersDTO searchParameters)
		{
			_logger.LogInformation($"{searchParameters.Text} {searchParameters.Sort} {searchParameters.TimeFilter} {searchParameters.LengthFilter}");
			var indicesResult = await _searchService.GetVideos(
				searchParameters.Text,
				searchParameters.Sort,
				searchParameters.TimeFilter,
				searchParameters.LengthFilter);
			if (indicesResult.IsError)
				return StatusCode(indicesResult.Code);
			var indicesData = indicesResult.GetRequiredObject();
			var videosResult = await _videoDataAccess.GetVideosByIds(indicesData.Select(_=>_.Id));
			IEnumerable<Video> videosResultSorted = indicesData.Join(
				videosResult, outer => outer.Id, inner => inner.Id.ToString(), (index, result)=> result);
			return Ok(videosResultSorted.Select(VideoDTO.FromModel));
		}
	}
}
