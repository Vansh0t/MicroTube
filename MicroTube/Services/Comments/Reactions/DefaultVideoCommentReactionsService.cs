using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Data.Models.Comments;
using MicroTube.Data.Models.Reactions;
using MicroTube.Services.Reactions;
using MicroTube.Services.VideoContent.Comments;
using System.Data;

namespace MicroTube.Services.Comments.Reactions
{
    public class DefaultVideoCommentReactionsService : ILikeDislikeReactionService
	{
		private readonly MicroTubeDbContext _db;
		private readonly ILogger<DefaultVideoCommentingService> _logger;
		private readonly ILikeDislikeReactionAggregator _reactionAggregator;

		public DefaultVideoCommentReactionsService(MicroTubeDbContext db, ILogger<DefaultVideoCommentingService> logger, ILikeDislikeReactionAggregator reactionAggregator)
		{
			_db = db;
			_logger = logger;
			_reactionAggregator = reactionAggregator;
		}
		public async Task<IServiceResult<ILikeDislikeReaction>> GetReaction(string userId, string commentId)
		{
			if (!Guid.TryParse(userId, out var guidUserId) || !Guid.TryParse(commentId, out var guidCommentId))
			{
				return ServiceResult<ILikeDislikeReaction>.Fail(400, "Invalid user or video id.");
			}
			var reaction = await _db.VideoCommentReactions.FirstOrDefaultAsync(_ => _.UserId == guidUserId && _.Id == guidCommentId);
			if (reaction == null)
			{
				return ServiceResult<ILikeDislikeReaction>.Fail(404, "Not found");
			}
			return ServiceResult<ILikeDislikeReaction>.Success(reaction);
		}
		public async Task<IServiceResult<ILikeDislikeReaction>> SetReaction(string userId, string commentId, LikeDislikeReactionType reactionType)
		{
			if (!Guid.TryParse(commentId, out var guidCommentId) || !Guid.TryParse(userId, out var guidUserId))
			{
				return ServiceResult<ILikeDislikeReaction>.Fail(400, "Invalid comment id or user id");
			}
			using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
			try
			{
				VideoComment? comment = await _db.VideoComments.Include(_ => _.Reactions).FirstOrDefaultAsync(_ => _.Id == guidCommentId);
				AppUser? user = await _db.Users.FirstOrDefaultAsync(_ => _.Id == guidUserId);
				if (user == null || !user.IsEmailConfirmed)
				{
					return ServiceResult<ILikeDislikeReaction>.Fail(403, "Forbidden");
				}
				if (comment == null || comment.Reactions == null)
				{
					return ServiceResult<ILikeDislikeReaction>.Fail(404, "Target comment does not exist or does not have reactions available.");
				}
				var reaction = await _db.VideoCommentReactions.FirstOrDefaultAsync(_ => _.UserId == guidUserId && _.CommentId == guidCommentId);
				if (reaction == null)
				{
					reaction = new VideoCommentReaction { ReactionType = LikeDislikeReactionType.None, Time = DateTime.UtcNow, Comment = comment, UserId = guidUserId };
					_db.Add(reaction);
				}
				comment.Reactions = (VideoCommentReactionsAggregation)_reactionAggregator.UpdateReactionsAggregation(comment.Reactions, reactionType, reaction.ReactionType);
				comment.Reactions.Difference = comment.Reactions.Likes - comment.Reactions.Dislikes;
				reaction.Time = DateTime.UtcNow;
				reaction.ReactionType = reactionType;
				_db.SaveChanges();
				await transaction.CommitAsync();
				return ServiceResult<ILikeDislikeReaction>.Success(reaction);
			}
			catch (Exception e)
			{
				await transaction.RollbackAsync();
				_logger.LogError(e, $"Failed to add comment reaction for comment {commentId}.");
				return ServiceResult<ILikeDislikeReaction>.FailInternal();
			}
		}
	}
}
