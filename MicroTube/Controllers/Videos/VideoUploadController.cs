using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicroTube.Controllers.Videos.DTO;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.Authentication;
using MicroTube.Services.VideoContent.Processing;

namespace MicroTube.Controllers.Videos
{
    [Route("Videos/[controller]")]
	[ApiController]
	public class VideoUploadController : ControllerBase
	{
		private readonly IJwtClaims _jwtClaims;
		private readonly MicroTubeDbContext _db;
		private readonly IVideoPreprocessingPipeline<VideoPreprocessingOptions, VideoUploadProgress> _preprocessingPipeline;
		public VideoUploadController(
			IJwtClaims jwtClaims,
			IVideoPreprocessingPipeline<VideoPreprocessingOptions, VideoUploadProgress> preprocessingPipeline,
			MicroTubeDbContext db)
		{
			_jwtClaims = jwtClaims;
			_preprocessingPipeline = preprocessingPipeline;
			_db = db;
		}

		[HttpPost]
		[Authorize]
		[DisableRequestSizeLimit]
		[RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue)]
		[ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(VideoUploadProgressDTO))]
		public async Task<IActionResult> Upload([FromForm] VideoUploadDTO uploadData)
		{
			bool isEmailConfirmed = _jwtClaims.GetIsEmailConfirmed(User);
			if (!isEmailConfirmed)
				return StatusCode(403, "Email confirmation is required for this action");
			string userId = _jwtClaims.GetUserId(User);
			var preprocessingData = new VideoPreprocessingOptions(userId, uploadData.Title, uploadData.Description, uploadData.File);
			var preprocessingResult = await _preprocessingPipeline.PreprocessVideo(preprocessingData);
			if (preprocessingResult.IsError)
				return StatusCode(preprocessingResult.Code, preprocessingResult.Error);
			var uploadProgress = preprocessingResult.GetRequiredObject();
			var result = new VideoUploadProgressDTO(
				userId,
				uploadProgress.Status,
				uploadData.Title,
				uploadProgress.Description,
				uploadProgress.Message,
				uploadProgress.Timestamp,
				uploadProgress.LengthSeconds,
				uploadProgress.Fps,
				uploadProgress.FrameSize,
				uploadProgress.Format);
			return Accepted(result);
		}
		[HttpGet("progress")]
		[Authorize]
		public async Task<IActionResult> GetProgressList()
		{
			string userId = _jwtClaims.GetUserId(User);
			var result = await _db.VideoUploadProgresses.Where(_ => _.UploaderId == new Guid(userId)).ToArrayAsync();
			return Ok(result.Select(_ => 
			new VideoUploadProgressDTO(
				_.Id.ToString(), 
				_.Status, 
				_.Title, 
				_.Description, 
				_.Message, 
				_.Timestamp,
				_.LengthSeconds,
				_.Fps,
				_.FrameSize,
				_.Format)));
		}
	}
}
