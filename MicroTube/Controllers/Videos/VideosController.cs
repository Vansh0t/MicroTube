using Microsoft.AspNetCore.Mvc;
using MicroTube.Controllers.Videos.DTO;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.Search;

namespace MicroTube.Controllers.Videos
{
	[Route("[controller]")]
	[ApiController]
	public class VideosController : ControllerBase
	{
		private readonly IVideoDataAccess _videoDataAccess;
		private readonly IVideoSearchService _searchService;
		public VideosController(
			IVideoDataAccess videoDataAccess, IVideoSearchService searchService)
		{
			_videoDataAccess = videoDataAccess;
			_searchService = searchService;
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
			var result = new VideoDTO
			{
				Id = video.Id.ToString(),
				Urls = video.Urls,
				Title = video.Title,
				Description = video.Description,
				SnapshotUrls = video.SnapshotUrls,
				ThumbnailUrls = video.ThumbnailUrls,
				UploadTime = video.UploadTime,
				LengthSeconds = video.LengthSeconds
			};
			return Accepted(result);
		}
	}
}
