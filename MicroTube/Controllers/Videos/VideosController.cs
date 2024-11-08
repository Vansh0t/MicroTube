using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicroTube.Controllers.Videos.Dto;
using MicroTube.Data.Access;
using MicroTube.Services.Authentication;
using MicroTube.Services.Search.Videos;
using MicroTube.Services.VideoContent.Likes;
using MicroTube.Services.VideoContent.Views;

namespace MicroTube.Controllers.Videos
{
	[Route("[controller]")]
	[ApiController]
	public class VideosController : ControllerBase
	{
		private readonly IVideoSearchService _searchService;
		private readonly IJwtClaims _jwtClaims;
		private readonly IVideoViewsAggregatorService _viewsService;
		private readonly MicroTubeDbContext _db;
		public VideosController(
			IVideoSearchService searchService,
			IJwtClaims jwtClaims,
			IVideoViewsAggregatorService viewsService,
			MicroTubeDbContext db)
		{
			_searchService = searchService;
			_jwtClaims = jwtClaims;
			_viewsService = viewsService;
			_db = db;
		}
		[HttpGet("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<VideoDto>))]
		public async Task<IActionResult> Get(string id)
		{
			var video = await _db.Videos
				.Select(_=> new VideoDto 
				{
					Id = _.Id.ToString(),
					Title = _.Title,
					Urls = _.Urls,
					Description = _.Description,
					UploadTime = _.UploadTime,
					ThumbnailUrls = _.ThumbnailUrls,
					LengthSeconds = _.LengthSeconds,
					Likes = _.VideoReactions != null ? _.VideoReactions.Likes : 0,
					Dislikes = _.VideoReactions != null ? _.VideoReactions.Dislikes : 0,
					Views = _.VideoViews != null ? _.VideoViews.Views : 0,
					UploaderPublicUsername = _.Uploader != null ? _.Uploader.PublicUsername : "Unknown",
					UploaderId = _.UploaderId.ToString()
				})
				.FirstOrDefaultAsync(_ => _.Id == id);
			if (video == null)
				return NotFound("Video not found");
			return Accepted(video);
		}
		[HttpPost("{id}/view")]
		[ProducesResponseType(StatusCodes.Status202Accepted)]
		public async Task<IActionResult> ReportView(string id)
		{
			string? ip = HttpContext.GetIp();
			if(ip == null)
			{
				return Forbid("Unable to read connection IP. The protocol might be invalid.");
			}
			var result = await _viewsService.CreateViewForAggregation(id, ip);
			return StatusCode(result.Code, result.Error);
		}
	}
}
