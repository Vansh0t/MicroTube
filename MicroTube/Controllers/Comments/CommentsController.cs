using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroTube.Controllers.Comments.Dto;
using MicroTube.Controllers.Reactions.Dto;
using MicroTube.Data.Access;
using MicroTube.Data.Models.Comments;
using MicroTube.Data.Models.Reactions;
using MicroTube.Services.Authentication;
using MicroTube.Services.Comments;
using MicroTube.Services.Comments.Reactions;
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
		private readonly ILogger<CommentsController> _logger;
		public CommentsController(
			IJwtClaims claims,
			MicroTubeDbContext db,
			IVideoCommentSearchService commentSearch,
			CommentServicesFactory serviceFactory,
			ILogger<CommentsController> logger)
		{
			_claims = claims;
			_db = db;
			_commentSearch = commentSearch;
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		[Authorize]
		[HttpPost("{targetKey}/{videoId}/comment")]
		public async Task<IActionResult> Comment(string targetKey, string videoId, CommentRequestDto commentRequest)
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
			var result = new CommentDto(comment.Id.ToString(), comment.Content, comment.Time, comment.Edited)
			{
				UserId = posterId,
				UserAlias = posterAlias,
				TargetId = comment.TargetId.ToString()
			};
			return Ok(result);
		}
		[Authorize]
		[HttpPost("{targetKey}/{commentId}/edit")]
		public async Task<IActionResult> Edit(string targetKey, string commentId, EditCommentRequestDto editRequest)
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
			var result = new CommentDto(comment.Id.ToString(), comment.Content, comment.Time, comment.Edited)
			{
				UserId = posterId,
				UserAlias = posterAlias,
				TargetId = targetId
			};
			result = await ResolveReactions(targetKey, userId, result, comment);
			return Ok(result);// reactions are not vital return Ok despite possible errors
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
			return Ok(new CommentDto(comment.Id.ToString(), comment.Content, comment.Time, comment.Edited) 
			{ 
				UserId = posterId, 
				UserAlias = posterAlias,
				TargetId = targetId 
			});
		}
		[HttpPost("video/{videoId}/get")]
		public async Task<IActionResult> GetComments(string videoId, [FromQuery] CommentSearchParametersDto searchParameters, [FromBody] CommentSearchMetaDto? meta)
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
			var dtoPairs = resultObject.Comments.Select(
					_ => new CommentDtoPair(new CommentDto(_.Id.ToString(), _.Content, _.Time, _.Edited)
						 {
						 	TargetId = _.Id.ToString(),
						 	UserAlias = _.User != null ? _.User.PublicUsername : "Unknown",
						 	UserId = _.UserId.ToString()
						 }, _)).ToArray();
			if(User.Identity != null && User.Identity.IsAuthenticated)
			{
				string userId = _claims.GetUserId(User);
				var updatedPairs = await ResolveReactionsBatch("video", userId, dtoPairs);
			}
			CommentSearchResultDto finalResult = new CommentSearchResultDto(dtoPairs.Select(_=>_.CommentDto), resultObject.Meta);
			return Ok(finalResult);
		}
		private async Task<CommentDto> ResolveReactions(string targetKey, string userId, CommentDto commentDto, IComment comment)
		{
			if (!_serviceFactory.TryGetCommentReactionsProviderService(targetKey, out var reactionsProvider))
			{
				_logger.LogCritical($"Unable to resolve reactions provider from {nameof(CommentServicesFactory)} for target {targetKey}");
				return commentDto;
			}
			CommentDto result;
			var dtoWithUserReactionResult = await reactionsProvider.ResolveUserReactionForCommentDto(userId, new CommentDtoPair(commentDto, comment));
			result = dtoWithUserReactionResult.IsError ? commentDto : dtoWithUserReactionResult.GetRequiredObject();
			var dtoWithReactionsAggregationResult = await reactionsProvider.ResolveReactionsAggregationForCommentDto(new CommentDtoPair(result, comment));
			result = dtoWithReactionsAggregationResult.IsError ? commentDto : dtoWithReactionsAggregationResult.GetRequiredObject();
			return result;
		}
		private async Task<IEnumerable<CommentDtoPair>> ResolveReactionsBatch(string targetKey, string userId, IEnumerable<CommentDtoPair> commentDtoPairs)
		{
			if (!_serviceFactory.TryGetCommentReactionsProviderService(targetKey, out var reactionsProvider))
			{
				_logger.LogCritical($"Unable to resolve reactions provider from {nameof(CommentServicesFactory)} for target {targetKey}");
				return new CommentDtoPair[0];
			}
			var dtoWithUserReactionResult = await reactionsProvider.ResolveUserReactionForCommentDto(userId, commentDtoPairs);
			var dtoWithReactionsAggregationResult = await reactionsProvider.ResolveReactionsAggregationForCommentDto(commentDtoPairs);
			return commentDtoPairs;
		}
	}
}
