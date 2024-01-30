using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroTube.Services;
using MicroTube.Services.Authentication;
using MicroTube.Services.Validation;
using MicroTube.Services.VideoContent;

namespace MicroTube.Controllers.Videos
{
	[Route("Videos/[controller]")]
	[ApiController]
	public class VideoUploadController : ControllerBase
	{
		private readonly IJwtClaims _jwtClaims;
		private readonly IVideoPreUploadValidator _preUploadValidator;
		private readonly IVideoContentLocalStorage _videoLocalStorage;
		public VideoUploadController(IJwtClaims jwtClaims, IVideoPreUploadValidator preUploadValidator, IVideoContentLocalStorage videoLocalStorage)
		{
			_jwtClaims = jwtClaims;
			_preUploadValidator = preUploadValidator;
			_videoLocalStorage = videoLocalStorage;
		}

		[HttpPost]
		[Authorize]
		[DisableRequestSizeLimit]
		[RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
		public async Task<IActionResult> Upload(IFormFile file)
		{
			bool isEmailConfirmed = _jwtClaims.GetIsEmailConfirmed(User);
			if (!isEmailConfirmed)
				return StatusCode(403, "Email confirmation is required for this action");
			IServiceResult validationResult = _preUploadValidator.Validate(file);
			if (validationResult.IsError)
				return StatusCode(validationResult.Code, validationResult.Error);
			using var stream = file.OpenReadStream();
			var localSaveResult = await _videoLocalStorage.Save(stream, file.FileName);
			if (localSaveResult.IsError)
				return StatusCode(localSaveResult.Code, localSaveResult.Error);
			return Ok();
		}
	}
}
