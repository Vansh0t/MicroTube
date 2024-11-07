using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroTube.Controllers.Comments.DTO;
using MicroTube.Data.Access;
using MicroTube.Services.Authentication;
using MicroTube.Services.Comments;
using MicroTube.Services.Search.Comments;

namespace MicroTube.Controllers.Comments
{
    [Route("[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
		private readonly IJwtClaims _claims;
		private readonly CommentServicesFactory _serviceFactory;
		private readonly MicroTubeDbContext _db;
		private readonly IVideoCommentSearchService _commentSearch;
		public CommentsController(IJwtClaims claims, MicroTubeDbContext db, IVideoCommentSearchService commentSearch, CommentServicesFactory serviceFactory)
		{
			_claims = claims;
			_db = db;
			_commentSearch = commentSearch;
			_serviceFactory = serviceFactory;
		}

		[Authorize]
		[HttpPost("{targetKey}/{videoId}/comment")]
		public async Task<IActionResult> Comment(string targetKey, string videoId, CommentRequestDTO commentRequest)
		{
			bool isEmailConfirmed = _claims.GetIsEmailConfirmed(User);
			if (!isEmailConfirmed)
			{
				return StatusCode(403, "Email confirmation is required for this action");
			}
			string userId = _claims.GetUserId(User);
			if(!_serviceFactory.TryGetCommentingService(targetKey, out var service))
			{
				return BadRequest($"Invalid comment target: {targetKey}");
			}
			var commentResult = await service!.Comment(userId, videoId, commentRequest.Content);
			if(commentResult.IsError)
			{
				return StatusCode(commentResult.Code, commentResult.Error);
			}
			var comment = commentResult.GetRequiredObject();
			string? posterId = comment.UserId.ToString();
			string? posterAlias = comment.User != null ? comment.User.PublicUsername : null;
			var result = new CommentDTO(comment.Id.ToString(), comment.Content, comment.Time, comment.Edited)
			{
				UserId = posterId,
				UserAlias = posterAlias,
				TargetId = comment.TargetId.ToString()
			};
			return Ok(result);
		}
		[Authorize]
		[HttpPost("{targetKey}/{commentId}/edit")]
		public async Task<IActionResult> Edit(string targetKey, string commentId, EditCommentRequestDTO editRequest)
		{
			string userId = _claims.GetUserId(User);
			if (!_serviceFactory.TryGetCommentingService(targetKey, out var service))
			{
				return BadRequest($"Invalid comment target: {targetKey}");
			}
			var commentResult = await service!.EditComment(userId, editRequest.NewContent, commentId);
			if (commentResult.IsError)
			{
				return StatusCode(commentResult.Code, commentResult.Error);
			}
			var comment = commentResult.GetRequiredObject();
			string? posterId = comment.UserId.ToString();
			string? posterAlias = comment.User != null ? comment.User.PublicUsername : null;
			string? targetId = comment.TargetId.ToString();
			return Ok(new CommentDTO(comment.Id.ToString(), comment.Content, comment.Time, comment.Edited)
			{ 
				UserId = posterId, 
				UserAlias = posterAlias, 
				TargetId = targetId 
			});
		}
		[Authorize]
		[HttpPost("{targetKey}/{commentId}/delete")]
		public async Task<IActionResult> Delete(string targetKey, string commentId)
		{
			string userId = _claims.GetUserId(User);
			if (!_serviceFactory.TryGetCommentingService(targetKey, out var service))
			{
				return BadRequest($"Invalid comment target: {targetKey}");
			}
			var commentResult = await service!.DeleteComment(userId, commentId);
			if (commentResult.IsError)
			{
				return StatusCode(commentResult.Code, commentResult.Error);
			}
			var comment = commentResult.GetRequiredObject();
			string? posterId = comment.UserId.ToString();
			string? posterAlias = comment.User != null ? comment.User.PublicUsername : null;
			string? targetId = comment.TargetId.ToString();
			return Ok(new CommentDTO(comment.Id.ToString(), comment.Content, comment.Time, comment.Edited) 
			{ 
				UserId = posterId, 
				UserAlias = posterAlias,
				TargetId = targetId 
			});
		}
		[HttpPost("video/{videoId}/get")]
		public async Task<IActionResult> GetComments(string videoId, [FromQuery] CommentSearchParametersDTO searchParameters, [FromBody] CommentSearchMetaDTO? meta)
		{
			VideoCommentSearchParameters parameters = new VideoCommentSearchParameters
			{
				BatchSize = searchParameters.BatchSize,
				SortType = searchParameters.SortType
			};
			var result = await _commentSearch.GetComments(videoId, parameters, meta != null?meta.Meta: null);
			if(result.IsError)
			{
				return StatusCode(result.Code, result.Error);
			}
			var resultObject = result.GetRequiredObject();
			CommentSearchResultDTO finalResult = new CommentSearchResultDTO(
				resultObject.Comments.Select(
					_=>new CommentDTO(_.Id.ToString(), _.Content, _.Time, _.Edited)
					{
						TargetId = _.Id.ToString(),
						UserAlias = _.User != null ? _.User.PublicUsername : "Unknown",
						UserId = _.UserId.ToString()
					})
				, resultObject.Meta);
			return Ok(finalResult);
		}
	}
}
