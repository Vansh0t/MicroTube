using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicroTube.Controllers.Videos.DTO;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.Authentication;
using MicroTube.Services.Reactions;
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
		[HttpGet("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<VideoDTO>))]
		public async Task<IActionResult> Get(string id)
		{
			var video = await _db.Videos
				.Select(_=> new VideoDTO 
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
		[HttpPost("{id}/reaction/{reactionType}")]
		[Authorize]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserVideoReactionDTO))]
		public async Task<IActionResult> React(string id, LikeDislikeReactionType reactionType)
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
