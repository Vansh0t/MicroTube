﻿using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroTube.Controllers.Authentication.DTO;
using MicroTube.Controllers.User.DTO;
using MicroTube.Controllers.Videos.DTO;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services;
using MicroTube.Services.Authentication;
using MicroTube.Services.VideoContent.Processing;

namespace MicroTube.Controllers.Videos
{
    [Route("Videos/[controller]")]
	[ApiController]
	public class VideoUploadController : ControllerBase
	{
		private readonly IJwtClaims _jwtClaims;
		private readonly IVideoDataAccess _videoDataAccess;
		private readonly IVideoPreprocessingPipeline<VideoPreprocessingOptions, VideoUploadProgress> _preprocessingPipeline;
		public VideoUploadController(
			IJwtClaims jwtClaims,
			IVideoDataAccess videoDataAccess,
			IVideoPreprocessingPipeline<VideoPreprocessingOptions, VideoUploadProgress> preprocessingPipeline)
		{
			_jwtClaims = jwtClaims;
			_videoDataAccess = videoDataAccess;
			_preprocessingPipeline = preprocessingPipeline;
		}

		[HttpPost]
		[Authorize]
		[DisableRequestSizeLimit]
		[RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
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
			var result = await _videoDataAccess.GetVideoUploadProgressListForUser(userId);
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
