using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicroTube.Controllers.Videos.Dto;
using MicroTube.Data.Access;
using MicroTube.Services.Authentication;
using MicroTube.Services.VideoContent.Preprocessing;
using MicroTube.Services.VideoContent.Preprocessing.Stages;
using MicroTube.Services.Videos;

namespace MicroTube.Controllers.Videos
{
    [Route("Videos/[controller]")]
	[ApiController]
	public class VideoUploadController : ControllerBase
	{
		private readonly IJwtClaims _jwtClaims;
		private readonly MicroTubeDbContext _db;
		private readonly IVideoPreprocessingPipeline _preprocessingPipeline;
		private readonly IVideoUploadLinkProvider _uploadLinkProvider;
		private readonly IRemoteStorageVideoMetaHandler _metaHandler;
		public VideoUploadController(
			IJwtClaims jwtClaims,
			MicroTubeDbContext db,
			IVideoPreprocessingPipeline preprocessingPipeline,
			IVideoUploadLinkProvider uploadLinkProvider,
			IRemoteStorageVideoMetaHandler metaHandler)
		{
			_jwtClaims = jwtClaims;
			_db = db;
			_preprocessingPipeline = preprocessingPipeline;
			_uploadLinkProvider = uploadLinkProvider;
			_metaHandler = metaHandler;
		}

		[HttpPost("link")]
		[Authorize]
		public async Task<IActionResult> GetUploadLink(VideoUploadDto uploadData)
		{
			bool isEmailConfirmed = _jwtClaims.GetIsEmailConfirmed(User);
			if (!isEmailConfirmed)
				return StatusCode(403, "Email confirmation is required for this action");
			string userId = _jwtClaims.GetUserId(User);
			Dictionary<string, string?> meta = new Dictionary<string, string?>();
			_metaHandler.WriteTitle(meta, uploadData.Title);
			_metaHandler.WriteDescription(meta, uploadData.Description);
			_metaHandler.WriteUploaderId(meta, userId);
			var linkResult = await _uploadLinkProvider.GetUploadLink(uploadData.FileName, meta);
			if(linkResult.IsError)
			{
				return StatusCode(linkResult.Code, linkResult.Error);
			}
			return Ok(linkResult.GetRequiredObject());
		}
		[HttpPost("notify")]
		[Authorize]
		public async Task<IActionResult> NotifyVideoSourceUploadComplete(VideoNotifyUploadDto notifyUploadData)
		{
			bool isEmailConfirmed = _jwtClaims.GetIsEmailConfirmed(User);
			if (!isEmailConfirmed)
				return StatusCode(403, "Email confirmation is required for this action");
			string userId = _jwtClaims.GetUserId(User);
			var preprocessingData = new VideoPreprocessingData(userId, notifyUploadData.GeneratedFileName, notifyUploadData.GeneratedLocationName);
			//TO DO: This needs better error handling
			try
			{
				var preprocessingResult = await _preprocessingPipeline.Execute(new DefaultVideoPreprocessingContext { PreprocessingData = preprocessingData });
				var uploadProgress = preprocessingResult.UploadProgress;
				Guard.Against.Null(uploadProgress);
				var result = new VideoUploadProgressDto(
					userId,
					uploadProgress.Status,
					uploadProgress.Title,
					uploadProgress.Description,
					uploadProgress.Message,
					uploadProgress.Timestamp,
					uploadProgress.LengthSeconds,
					uploadProgress.Fps,
					uploadProgress.FrameSize,
					uploadProgress.Format);
				return Accepted(result);
			}
			catch (RequiredObjectNotFoundException)
			{
				return NotFound("Video file was not found in remote location");
			}
			
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
