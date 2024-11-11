using MicroTube.Controllers.Comments.Dto;
using MicroTube.Data.Models.Comments;

namespace MicroTube.Services.Comments.Reactions
{
	public interface ICommentReactionsProvider
	{
		Task<IServiceResult<CommentDto>> ResolveReactionsAggregationForCommentDto(CommentDtoPair commentDtoPair);
		Task<IServiceResult<CommentDto>> ResolveUserReactionForCommentDto(string userId, CommentDtoPair commentDtoPair);
		Task<IServiceResult<IEnumerable<CommentDto>>> ResolveReactionsAggregationForCommentDto(IEnumerable<CommentDtoPair> commentDtoPairs);
		Task<IServiceResult<IEnumerable<CommentDto>>> ResolveUserReactionForCommentDto(string userId, IEnumerable<CommentDtoPair> commentDtoPairs);
	}
}