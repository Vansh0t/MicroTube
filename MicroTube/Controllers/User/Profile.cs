using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroTube.Controllers.User.DTO;
using MicroTube.Data.Access;
using MicroTube.Services.Authentication;

namespace MicroTube.Controllers.User
{
    [ApiController]
    [Route("User/[controller]")]
    public class Profile : ControllerBase
    {
        private readonly ILogger<Profile> _logger;
        private readonly IJwtClaims _claims;
		private readonly IAppUserDataAccess _dataAccess;

        public Profile(ILogger<Profile> logger, IJwtClaims claims, IAppUserDataAccess dataAccess)
        {
            _logger = logger;
            _dataAccess = dataAccess;
            _claims = claims;
        }
		[HttpGet]
		[Authorize]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDTO))]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetUser()
		{
			var userId = _claims.GetUserId(User);
			var user = await _dataAccess.Get(userId);
			if (user == null)
				return NotFound("User does not exists");
			return Ok(new UserDTO(user.Id.ToString(), user.Username, user.Email, user.PublicUsername, user.IsEmailConfirmed ));
		}
        
    }
}