using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicroTube.Controllers.Videos.Dto;
using MicroTube.Data.Access;
using MicroTube.Services.Authentication;
using MicroTube.Services.VideoContent.Preprocessing;
using MicroTube.Services.VideoContent.Preprocessing.Stages;

namespace MicroTube.Controllers.Videos
{
    [Route("Videos/[controller]")]
	[ApiController]
	public class VideoUploadController : ControllerBase
	{
		private readonly IJwtClaims _jwtClaims;
		private readonly MicroTubeDbContext _db;
		private readonly IVideoPreprocessingPipeline _preprocessingPipeline;
		public VideoUploadController(
			IJwtClaims jwtClaims,
			MicroTubeDbContext db,
			IVideoPreprocessingPipeline preprocessingPipeline)
		{
			_jwtClaims = jwtClaims;
			_db = db;
			_preprocessingPipeline = preprocessingPipeline;
		}

		[HttpPost]
		[Authorize]
		[DisableRequestSizeLimit]
		[RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue)]
		[ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(VideoUploadProgressDto))]
		public async Task<IActionResult> Upload([FromForm] VideoUploadDto uploadData)
		{
			bool isEmailConfirmed = _jwtClaims.GetIsEmailConfirmed(User);
			if (!isEmailConfirmed)
				return StatusCode(403, "Email confirmation is required for this action");
			string userId = _jwtClaims.GetUserId(User);
			var preprocessingData = new VideoPreprocessingData(userId, uploadData.Title, uploadData.Description, uploadData.File);
			var preprocessingResult = await _preprocessingPipeline.Execute(new DefaultVideoPreprocessingContext {PreprocessingData = preprocessingData });
			var uploadProgress = preprocessingResult.UploadProgress;
			Guard.Against.Null(uploadProgress);
			var result = new VideoUploadProgressDto(
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
			new VideoUploadProgressDto(
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
