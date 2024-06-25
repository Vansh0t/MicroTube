using Hangfire;
using Microsoft.AspNetCore.Mvc;
using MicroTube.Controllers.Videos.DTO;
using MicroTube.Services.Search;

namespace MicroTube.Controllers.Videos
{
    [Route("Videos/[controller]")]
	[ApiController]
	public class VideosSearchController : ControllerBase
	{
		private readonly IVideoSearchService _searchService;
		public VideosSearchController(IVideoSearchService searchService)
		{
			_searchService = searchService;
		}

		[HttpGet("suggestions/{text}")]
		public async Task<IActionResult> GetProgressList(string text)
		{
			var result = await _searchService.GetSuggestions(text);
			if (result.IsError)
				return StatusCode(result.Code);
			return Ok(result.GetRequiredObject().Select(_ => {
				return new VideoSearchSuggestion(
								_.Id.ToString(),
								_.Title);
			}));
		}
	}
}
