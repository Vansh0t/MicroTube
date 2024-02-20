using Microsoft.AspNetCore.Mvc;
using MicroTube.Controllers.Videos.DTO;
using MicroTube.Data.Access;

namespace MicroTube.Controllers.Videos
{
	[Route("[controller]")]
	[ApiController]
	public class VideosController : ControllerBase
	{
		private readonly IVideoDataAccess _videoDataAccess;
		public VideosController(
			IVideoDataAccess videoDataAccess)
		{
			_videoDataAccess = videoDataAccess;
		}

		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<VideoDTO>))]
		public async Task<IActionResult> GetAll()
		{
			var videos = await _videoDataAccess.GetVideos();
			var result = videos.Select(_ =>
				new VideoDTO
				{
					Id = _.Id.ToString(),
					Url = _.Url,
					Title = _.Title,
					Description = _.Description,
					SnapshotUrls = _.SnapshotUrls,
					ThumbnailUrls = _.ThumbnailUrls,
					UploadTime = _.UploadTime
				});
			return Accepted(result);
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
				Url = video.Url,
				Title = video.Title,
				Description = video.Description,
				SnapshotUrls = video.SnapshotUrls,
				ThumbnailUrls = video.ThumbnailUrls,
				UploadTime = video.UploadTime
			};
			return Accepted(result);
		}
	}
}
