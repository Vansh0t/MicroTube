using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroTube.Controllers.Reactions.DTO;
using MicroTube.Services.Authentication;
using MicroTube.Services.Reactions;

namespace MicroTube.Controllers.Reactions
{
    [Route("[controller]")]
    [ApiController]
    public class ReactionsController : ControllerBase
    {
		private readonly IJwtClaims _jwtClaims;
		private readonly ReactionServicesFactory _serviceFactory;

		public ReactionsController(IJwtClaims jwtClaims, ReactionServicesFactory serviceFactory)
		{
			_jwtClaims = jwtClaims;
			_serviceFactory = serviceFactory;
		}

		[HttpPost("{targetKey}/{id}/react/{reactionType}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LikeDislikeReactionDTO))]
        public async Task<IActionResult> React(string targetKey, string id, LikeDislikeReactionType reactionType)
        {
			bool isEmailConfirmed = _jwtClaims.GetIsEmailConfirmed(User);
            if (!isEmailConfirmed)
			{
                return StatusCode(403, "Email confirmation is required for this action");
			}
			if (!_serviceFactory.TryGetLikeDislikeService(targetKey, out var service))
			{
				return BadRequest($"Invalid reaction target: {targetKey}");
			}
            string userId = _jwtClaims.GetUserId(User);
            var result = await service!.SetReaction(userId, id, reactionType);
            if (result.IsError)
                return StatusCode(result.Code, result.Error);
            return Ok(LikeDislikeReactionDTO.FromModel(result.GetRequiredObject()));
        }
        [HttpGet("{targetKey}/{id}/reaction")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LikeDislikeReactionDTO))]
        public async Task<IActionResult> GetReaction(string targetKey, string id)
        {
			bool isEmailConfirmed = _jwtClaims.GetIsEmailConfirmed(User);
			if (!isEmailConfirmed)
			{
				return StatusCode(403, "Email confirmation is required for this action");
			}
			if (!_serviceFactory.TryGetLikeDislikeService(targetKey, out var service))
			{
				return BadRequest($"Invalid reaction target: {targetKey}");
			}
			string userId = _jwtClaims.GetUserId(User);
            var likeResult = await service!.GetReaction(userId, id);
            if (likeResult.IsError)
                return StatusCode(likeResult.Code, likeResult.Code);
            return Ok(LikeDislikeReactionDTO.FromModel(likeResult.GetRequiredObject()));
        }
    }
}
