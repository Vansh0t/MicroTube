using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroTube.Controllers.Videos.DTO;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.Authentication;
using MicroTube.Services.Search;
using MicroTube.Services.VideoContent.Likes;

namespace MicroTube.Controllers.Videos
{
	[Route("[controller]")]
	[ApiController]
	public class VideosController : ControllerBase
	{
		private readonly IVideoDataAccess _videoDataAccess;
		private readonly IVideoSearchService _searchService;
		private readonly IJwtClaims _jwtClaims;
		private readonly IVideoLikesService _likesService;
		public VideosController(
			IVideoDataAccess videoDataAccess, IVideoSearchService searchService, IJwtClaims jwtClaims, IVideoLikesService likesService)
		{
			_videoDataAccess = videoDataAccess;
			_searchService = searchService;
			_jwtClaims = jwtClaims;
			_likesService = likesService;
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VideoSearchResultDTO))]
		public async Task<IActionResult> GetAll(VideoRequestMetaDTO? meta)
		{
			var searchResult = await _searchService.GetVideos(new VideoSearchParameters()
			{
				Text = null,
				SortType = VideoSortType.Time
			}, meta != null ? meta.Meta : null);
			if (searchResult.IsError)
				return StatusCode(searchResult.Code);
			var searchData = searchResult.GetRequiredObject();
			var videosResult = await _videoDataAccess.GetVideosByIds(searchData.Indices.Select(_ => _.Id));
			IEnumerable<Video> videosResultSorted = searchData.Indices.Join(
				videosResult, outer => outer.Id, inner => inner.Id.ToString(), (index, result) => result);
			var sortedVideos = videosResultSorted.Select(VideoDTO.FromModel);
			return Ok(new VideoSearchResultDTO(sortedVideos) { Meta = searchData.Meta});
		}
		[HttpGet("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<VideoDTO>))]
		public async Task<IActionResult> Get(string id)
		{
			var video = await _videoDataAccess.GetVideo(id);
			if (video == null)
				return NotFound("Video not found");
			var result = VideoDTO.FromModel(video);
			return Accepted(result);
		}
		[HttpPost("{id}/like")]
		[Authorize]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VideoLikeDTO))]
		public async Task<IActionResult> Like(string id)
		{
			bool isEmailConfirmed = _jwtClaims.GetIsEmailConfirmed(User);
			if (!isEmailConfirmed)
				return StatusCode(403, "Email confirmation is required for this action");
			string userId = _jwtClaims.GetUserId(User);
			var likeResult = await _likesService.LikeVideo(userId, id);
			if (likeResult.IsError)
				return StatusCode(likeResult.Code, likeResult.Code);
			return Ok(VideoLikeDTO.FromModel(likeResult.GetRequiredObject()));
		}
		[HttpGet("{id}/like")]
		[Authorize]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VideoLikeDTO))]
		public async Task<IActionResult> GetLike(string id)
		{
			bool isEmailConfirmed = _jwtClaims.GetIsEmailConfirmed(User);
			if (!isEmailConfirmed)
				return StatusCode(403, "Email confirmation is required for this action");
			string userId = _jwtClaims.GetUserId(User);
			var likeResult = await _likesService.GetLike(userId, id);
			if (likeResult.IsError)
				return StatusCode(likeResult.Code, likeResult.Code);
			return Ok(VideoLikeDTO.FromModel(likeResult.GetRequiredObject()));
		}
		[HttpDelete("{id}/like")]
		[Authorize]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VideoLikeDTO))]
		public async Task<IActionResult> DeleteLike(string id)
		{
			bool isEmailConfirmed = _jwtClaims.GetIsEmailConfirmed(User);
			if (!isEmailConfirmed)
				return StatusCode(403, "Email confirmation is required for this action");
			string userId = _jwtClaims.GetUserId(User);
			var likeResult = await _likesService.UnlikeVideo(userId, id);
			return StatusCode(likeResult.Code, likeResult.Error);
		}
	}
}
