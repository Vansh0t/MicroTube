using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Data.Models.Comments;
using MicroTube.Data.Models.Reactions;
using MicroTube.Services.Reactions;
using System.Data;

namespace MicroTube.Services.Comments.Reactions
{
    public class LikeDislikeVideoCommentReactionsService : ILikeDislikeReactionService
	{
		private readonly MicroTubeDbContext _db;
		private readonly ILogger<DefaultVideoCommentingService> _logger;
		private readonly ILikeDislikeReactionAggregationHandler _aggregationHandler;

		public LikeDislikeVideoCommentReactionsService(MicroTubeDbContext db, ILogger<DefaultVideoCommentingService> logger, ILikeDislikeReactionAggregationHandler reactionAggregator)
		{
			_db = db;
			_logger = logger;
			_aggregationHandler = reactionAggregator;
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
			using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
			try
			{
				var validUserExists = await _db.Users.AnyAsync(_ => _.Id == guidUserId && _.IsEmailConfirmed);
				if(!validUserExists)
				{
					return ServiceResult<ILikeDislikeReaction>.Fail(403, "Email confirmation is required for this action");
				}
				var reaction = await _db.VideoCommentReactions.FirstOrDefaultAsync(_ => _.UserId == guidUserId && _.CommentId == guidCommentId);
				var commentExists = await _db.VideoComments.AnyAsync(_ => _.Id == guidCommentId);
				if(!commentExists)
				{
					return ServiceResult<ILikeDislikeReaction>.Fail(404, "Target comment does not exist.");
				}
				if (reaction == null)
				{
					reaction = new VideoCommentReaction { ReactionType = LikeDislikeReactionType.None, Time = DateTime.UtcNow, CommentId = guidCommentId, UserId = guidUserId };
					_db.Add(reaction);
				}
				await UpdateReactionsAggregation(guidCommentId, reactionType, reaction.ReactionType);
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
		private async Task UpdateReactionsAggregation(Guid commentId, LikeDislikeReactionType reactionType, LikeDislikeReactionType prevReactionType)
		{
			var aggregationChanges = _aggregationHandler.GetAggregationChange(reactionType, prevReactionType);
			if (aggregationChanges.DislikesChange != 0 || aggregationChanges.LikesChange != 0)
			{
				int rowsAffected = await _db.VideoCommentAggregatedReactions
					.Where(_ => _.CommentId == commentId)
					.ExecuteUpdateAsync(_ => _.SetProperty(__ => __.Likes, __ => __.Likes + aggregationChanges.LikesChange)
										.SetProperty(__ => __.Dislikes, __ => __.Dislikes + aggregationChanges.DislikesChange));
				if(rowsAffected == 0)
				{
					throw new DataAccessException("No rows were affected by the aggregation update.");
				}
			}
		}
	}
}
