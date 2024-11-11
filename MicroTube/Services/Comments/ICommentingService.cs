using MicroTube.Data.Models.Comments;
namespace MicroTube.Services.Comments
{
	public interface ICommentingService
	{
		Task<IServiceResult<IComment>> Comment(string userId, string targetId, string content);
		Task<IServiceResult<IComment>> DeleteComment(string userId, string commentId);
		Task<IServiceResult<IComment>> EditComment(string userId, string newContent, string commentId);
	}
}