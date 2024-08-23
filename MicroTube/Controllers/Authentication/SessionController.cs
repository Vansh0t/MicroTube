using Microsoft.AspNetCore.Mvc;
using MicroTube.Controllers.Authentication.DTO;
using MicroTube.Services.Authentication;

namespace MicroTube.Controllers.Authentication
{
    [ApiController]
    [Route("Authentication/[controller]")]
    public class SessionController : ControllerBase
    {
		private readonly IUserSessionService _userSession;
		private readonly IConfiguration _config;

		public SessionController(IUserSessionService userSession, IConfiguration config)
		{
			_userSession = userSession;
			_config = config;
		}

		[HttpPost("Refresh")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationResponseDTO))]
		public async Task<IActionResult> Refresh()
		{
			if(!HttpContext.Request.Cookies.TryGetValue(Constants.AuthorizationConstants.REFRESH_TOKEN_COOKIE_KEY, out string? refreshToken) 
				|| string.IsNullOrWhiteSpace(refreshToken))
			{
				return Forbid();
			}
			var refreshResult = await _userSession.RefreshSession(refreshToken);
			if (refreshResult.IsError)
				return StatusCode(refreshResult.Code, refreshResult.Error);
			var newSession = refreshResult.GetRequiredObject();

			HttpContext.AddRefreshTokenCookie(_config, newSession.RefreshTokenRaw, newSession.Session.Expiration);
			return Ok(new AuthenticationResponseDTO(newSession.AccessToken));
		}
    }
}