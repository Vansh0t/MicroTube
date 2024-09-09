using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicroTube.Controllers.Videos.DTO;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.Authentication;
using MicroTube.Services.Search;
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
		private readonly IVideoReactionsService _videoReactions;
		public VideosController(
			IVideoSearchService searchService,
			IJwtClaims jwtClaims,
			IVideoViewsAggregatorService viewsService,
			MicroTubeDbContext db,
			IVideoReactionsService videoReactions)
		{
			_searchService = searchService;
			_jwtClaims = jwtClaims;
			_viewsService = viewsService;
			_db = db;
			_videoReactions = videoReactions;
		}

		//[HttpPost]
		//[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VideoSearchResultDTO))]
		//public async Task<IActionResult> GetAll(VideoRequestMetaDTO? meta)
		//{
		//	var searchResult = await _searchService.GetVideos(new VideoSearchParameters()
		//	{
		//		Text = null,
		//		SortType = VideoSortType.Time
		//	}, meta != null ? meta.Meta : null);
		//	if (searchResult.IsError)
		//		return StatusCode(searchResult.Code);
		//	var searchData = searchResult.GetRequiredObject();
		//	var videosResult = await _videoDataAccess.GetVideosByIds(searchData.Indices.Select(_ => _.Id));
		//	IEnumerable<Video> videosResultSorted = searchData.Indices.Join(
		//		videosResult, outer => outer.Id, inner => inner.Id.ToString(), (index, result) => result);
		//	var sortedVideos = videosResultSorted.Select(VideoDTO.FromModel);
		//	return Ok(new VideoSearchResultDTO(sortedVideos) { Meta = searchData.Meta});
		//}
		[HttpGet("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<VideoDTO>))]
		public async Task<IActionResult> Get(string id)
		{
			var video = await _db.Videos.FirstOrDefaultAsync(_ => _.Id == new Guid(id));
			if (video == null)
				return NotFound("Video not found");
			var result = VideoDTO.FromModel(video);
			return Accepted(result);
		}
		[HttpPost("{id}/reaction/{reactionType}")]
		[Authorize]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserVideoReactionDTO))]
		public async Task<IActionResult> React(string id, ReactionType reactionType)
		{
			bool isEmailConfirmed = _jwtClaims.GetIsEmailConfirmed(User);
			if (!isEmailConfirmed)
				return StatusCode(403, "Email confirmation is required for this action");
			string userId = _jwtClaims.GetUserId(User);
			var result = await _videoReactions.SetReaction(userId, id, reactionType);
			if (result.IsError)
				return StatusCode(result.Code, result.Error);
			return Ok(UserVideoReactionDTO.FromModel(result.GetRequiredObject()));
		}
		[HttpGet("{id}/reaction")]
		[Authorize]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserVideoReactionDTO))]
		public async Task<IActionResult> GetReaction(string id)
		{
			bool isEmailConfirmed = _jwtClaims.GetIsEmailConfirmed(User);
			if (!isEmailConfirmed)
				return StatusCode(403, "Email confirmation is required for this action");
			string userId = _jwtClaims.GetUserId(User);
			var likeResult = await _videoReactions.GetReaction(userId, id);
			if (likeResult.IsError)
				return StatusCode(likeResult.Code, likeResult.Code);
			return Ok(UserVideoReactionDTO.FromModel(likeResult.GetRequiredObject()));
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
