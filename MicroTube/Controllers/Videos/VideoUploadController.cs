using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroTube.Controllers.Authentication.DTO;
using MicroTube.Data.Access;
using MicroTube.Services;
using MicroTube.Services.Authentication;
using MicroTube.Services.Validation;
using MicroTube.Services.VideoContent;
using MicroTube.Services.VideoContent.Processing;

namespace MicroTube.Controllers.Videos
{
	[Route("Videos/[controller]")]
	[ApiController]
	public class VideoUploadController : ControllerBase
	{
		private readonly IJwtClaims _jwtClaims;
		private readonly IVideoPreUploadValidator _preUploadValidator;
		private readonly IVideoContentLocalStorage _videoLocalStorage;
		private readonly IVideoDataAccess _videoDataAccess;
		private readonly IVideoProcessingQueue _videoProcessingQueue;
		private readonly ILogger<VideoUploadController> _logger;
		public VideoUploadController(
			IJwtClaims jwtClaims,
			IVideoPreUploadValidator preUploadValidator,
			IVideoContentLocalStorage videoLocalStorage,
			IVideoDataAccess videoDataAccess,
			IVideoProcessingQueue videoProcessingQueue,
			ILogger<VideoUploadController> logger)
		{
			_jwtClaims = jwtClaims;
			_preUploadValidator = preUploadValidator;
			_videoLocalStorage = videoLocalStorage;
			_videoDataAccess = videoDataAccess;
			_videoProcessingQueue = videoProcessingQueue;
			_logger = logger;
		}

		[HttpPost]
		[Authorize]
		[DisableRequestSizeLimit]
		[RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
		public async Task<IActionResult> Upload([FromForm] VideoUploadDTO uploadData)
		{
			bool isEmailConfirmed = _jwtClaims.GetIsEmailConfirmed(User);
			if (!isEmailConfirmed)
				return StatusCode(403, "Email confirmation is required for this action");
			string userId = _jwtClaims.GetUserId(User);
			IServiceResult fileValidationResult = _preUploadValidator.ValidateFile(uploadData.File);
			IServiceResult titleValidationResult = _preUploadValidator.ValidateTitle(uploadData.Title);
			IServiceResult descriptionValidationResult = _preUploadValidator.ValidateTitle(uploadData.Title);
			string joinedError = "";
			if (fileValidationResult.IsError)
				joinedError += fileValidationResult.Error;
			if (titleValidationResult.IsError)
				joinedError += ", " + titleValidationResult.Error;
			if (descriptionValidationResult.IsError)
				joinedError += ", " + descriptionValidationResult.Error;
			if (!string.IsNullOrWhiteSpace(joinedError))
				return BadRequest(joinedError);
			var file = uploadData.File;
			using var stream = file.OpenReadStream();
			var localSaveResult = await _videoLocalStorage.Save(stream, file.FileName);
			if (localSaveResult.IsError)
				return StatusCode(localSaveResult.Code, localSaveResult.Error);
			var localSave = localSaveResult.GetRequiredObject();
			try
			{
				var uploadProgressEntry = await _videoDataAccess.CreateUploadProgress(localSave.FullPath, userId, uploadData.Title, uploadData.Description);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Failed to create a video upload entry. Cancelling the processing and deleting from local storage.");
				_videoLocalStorage.TryDelete(localSave.FullPath);
				return StatusCode(500);
			}
			_videoProcessingQueue.EnqueueForProcessing(localSave.FullPath);
			return Ok();
		}
		[HttpGet("progress")]
		[Authorize]
		public async Task<IActionResult> GetProgressList()
		{
			string userId = _jwtClaims.GetUserId(User);
			var result = await _videoDataAccess.GetVideoUploadProgressListForUser(userId);
			return Ok(result.Select(_ => new VideoUploadProgressDTO(_.Id.ToString(), _.Status, _.Title, _.Description)));
		}
	}
}
