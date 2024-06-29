using Hangfire;
using Microsoft.AspNetCore.Mvc;
using MicroTube.Controllers.Videos.DTO;
using MicroTube.Data.Access;
using MicroTube.Services.Search;

namespace MicroTube.Controllers.Videos
{
    [Route("Videos/[controller]")]
	[ApiController]
	public class VideosSearchController : ControllerBase
	{
		private readonly IVideoSearchService _searchService;
		private readonly IVideoDataAccess _videoDataAccess;
		public VideosSearchController(IVideoSearchService searchService, IVideoDataAccess videoDataAccess)
		{
			_searchService = searchService;
			_videoDataAccess = videoDataAccess;
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
		[HttpGet("videos/{text}")]
		public async Task<IActionResult> GetVideos(string text)
		{
			var indicesResult = await _searchService.GetVideos(text);
			if (indicesResult.IsError)
				return StatusCode(indicesResult.Code);
			var videosResult = await _videoDataAccess.GetVideosByIds(indicesResult.GetRequiredObject().Select(_=>_.Id));
			return Ok(videosResult.Select(VideoDTO.FromModel));
		}
	}
}
