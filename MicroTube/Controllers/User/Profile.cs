using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicroTube.Controllers.User.Dto;
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
		private readonly MicroTubeDbContext _db;

		public Profile(ILogger<Profile> logger, IJwtClaims claims, MicroTubeDbContext db)
		{
			_logger = logger;
			_claims = claims;
			_db = db;
		}
		[HttpGet]
		[Authorize]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetUser()
		{
			var userId = _claims.GetUserId(User);
			var user = await _db.Users.FirstOrDefaultAsync(_ => _.Id == new Guid(userId));
			if (user == null)
				return NotFound("User does not exists");
			return Ok(new UserDto(user.Id.ToString(), user.Username, user.Email, user.PublicUsername, user.IsEmailConfirmed ));
		}
        
    }
}