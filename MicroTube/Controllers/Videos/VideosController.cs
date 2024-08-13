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
		private readonly IVideoDislikesService _dislikesService;
		public VideosController(
			IVideoDataAccess videoDataAccess, IVideoSearchService searchService, IJwtClaims jwtClaims, IVideoLikesService likesService, IVideoDislikesService dislikesService)
		{
			_videoDataAccess = videoDataAccess;
			_searchService = searchService;
			_jwtClaims = jwtClaims;
			_likesService = likesService;
			_dislikesService = dislikesService;
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
			var undislikeResult = await _dislikesService.UndislikeVideo(userId, id);
			if (undislikeResult.IsError && undislikeResult.Code != 404)
				return StatusCode(undislikeResult.Code, undislikeResult.Error);
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
		[HttpPost("{id}/dislike")]
		[Authorize]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VideoDislikeDTO))]
		public async Task<IActionResult> Dislike(string id)
		{
			bool isEmailConfirmed = _jwtClaims.GetIsEmailConfirmed(User);
			if (!isEmailConfirmed)
				return StatusCode(403, "Email confirmation is required for this action");
			string userId = _jwtClaims.GetUserId(User);
			var unlikeResult = await _likesService.UnlikeVideo(userId, id);
			if (unlikeResult.IsError && unlikeResult.Code != 404)
				return StatusCode(unlikeResult.Code, unlikeResult.Error);
			var result = await _dislikesService.DislikeVideo(userId, id);
			if (result.IsError)
				return StatusCode(result.Code, result.Code);
			return Ok(VideoDislikeDTO.FromModel(result.GetRequiredObject()));
		}
		[HttpGet("{id}/dislike")]
		[Authorize]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VideoDislikeDTO))]
		public async Task<IActionResult> GetDislike(string id)
		{
			bool isEmailConfirmed = _jwtClaims.GetIsEmailConfirmed(User);
			if (!isEmailConfirmed)
				return StatusCode(403, "Email confirmation is required for this action");
			string userId = _jwtClaims.GetUserId(User);
			var result = await _dislikesService.GetDislike(userId, id);
			if (result.IsError)
				return StatusCode(result.Code, result.Code);
			return Ok(VideoDislikeDTO.FromModel(result.GetRequiredObject()));
		}
		[HttpDelete("{id}/dislike")]
		[Authorize]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VideoDislikeDTO))]
		public async Task<IActionResult> DeleteDislike(string id)
		{
			bool isEmailConfirmed = _jwtClaims.GetIsEmailConfirmed(User);
			if (!isEmailConfirmed)
				return StatusCode(403, "Email confirmation is required for this action");
			string userId = _jwtClaims.GetUserId(User);
			var result = await _dislikesService.UndislikeVideo(userId, id);
			return StatusCode(result.Code, result.Error);
		}
	}
}
