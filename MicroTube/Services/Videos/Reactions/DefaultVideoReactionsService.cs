using Microsoft.Data.SqlClient;
using System.Data;
using MicroTube.Data.Access;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MicroTube.Services.Reactions;
using MicroTube.Data.Models.Reactions;
using MicroTube.Data.Models.Videos;

namespace MicroTube.Services.VideoContent.Likes
{
    public class DefaultVideoReactionsService : ILikeDislikeReactionService
	{
		private readonly ILogger<DefaultVideoReactionsService> _logger;
		private readonly ILikeDislikeReactionAggregationHandler _aggregationHandler;
		private readonly MicroTubeDbContext _db;
		public DefaultVideoReactionsService(
			ILogger<DefaultVideoReactionsService> logger,
			MicroTubeDbContext db,
			ILikeDislikeReactionAggregationHandler aggregationHandler)
		{
			_logger = logger;
			_db = db;
			_aggregationHandler = aggregationHandler;
		}

		public async Task<IServiceResult<ILikeDislikeReaction>> SetReaction(string userId, string videoId, LikeDislikeReactionType reactionType)
		{
			if(!Guid.TryParse(userId, out var guidUserId) || !Guid.TryParse(videoId, out var guidVideoId))
			{
				return ServiceResult<ILikeDislikeReaction>.Fail(400, "Invalid user or video id.");
			} 
			using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
			try
			{
				var validUserExists = await _db.Users.AnyAsync(_ => _.Id == guidUserId && _.IsEmailConfirmed);
				if (!validUserExists)
				{
					return ServiceResult<ILikeDislikeReaction>.Fail(403, "Email confirmation is required for this action");
				}
				bool videoExists = await _db.Videos.AnyAsync(_ => _.Id == guidVideoId);
				if (!videoExists)
				{
					return ServiceResult<ILikeDislikeReaction>.Fail(404, "Target video does not exist.");
				}
				(VideoReaction reaction, bool created) = await GetOrCreateReaction(guidUserId, guidVideoId);
				if (created)
				{
					_db.Add(reaction);
				}
				else if(reaction.ReactionType == reactionType)
				{
					return ServiceResult<ILikeDislikeReaction>.Success(reaction);
				}
				await UpdateReactionsAggregation(guidVideoId, reactionType, reaction.ReactionType);
				reaction.ReactionType = reactionType;
				await SetReindexingRequired(guidVideoId);
				_db.SaveChanges();
				await transaction.CommitAsync();
				return ServiceResult<ILikeDislikeReaction>.Success(reaction);
			}
			catch (RequiredObjectNotFoundException e)
			{
				await transaction.RollbackAsync();
				_logger.LogError(e, $"Failed add video like from user {userId} to video {videoId}");
				return ServiceResult<ILikeDislikeReaction>.Fail(404, "Requested video is not found");
			}
			catch (Exception e)
			{
				await transaction.RollbackAsync();
				_logger.LogError(e, $"Failed add video like from user {userId} to video {videoId}");
				return ServiceResult<ILikeDislikeReaction>.FailInternal();
			}
		}
		public async Task<IServiceResult<ILikeDislikeReaction>> GetReaction(string userId, string videoId)
		{
			if (!Guid.TryParse(userId, out var guidUserId) || !Guid.TryParse(videoId, out var guidVideoId))
			{
				return ServiceResult<ILikeDislikeReaction>.Fail(400, "Invalid user or video id.");
			}
			var reaction = await _db.VideoReactions.FirstOrDefaultAsync(_ => _.UserId == guidUserId && _.VideoId == guidVideoId);
			if (reaction == null)
				return ServiceResult<ILikeDislikeReaction>.Fail(404, "Not found");
			return ServiceResult<ILikeDislikeReaction>.Success(reaction);
		}
		private async Task<(VideoReaction reaction, bool created)> GetOrCreateReaction(Guid userId, Guid videoId)
		{
			var reaction = await _db.VideoReactions.FirstOrDefaultAsync(_ => _.UserId == userId && _.VideoId == videoId);
			if (reaction == null)
			{
				reaction = new VideoReaction { UserId = userId, VideoId = videoId, ReactionType = LikeDislikeReactionType.None, Time = DateTime.UtcNow };
				return (reaction, true);
			}
			return (reaction, false);
		}
		private async Task UpdateReactionsAggregation(Guid videoId, LikeDislikeReactionType reactionType, LikeDislikeReactionType prevReactionType)
		{
			var aggregationChanges = _aggregationHandler.GetAggregationChange(reactionType, prevReactionType);
			if (aggregationChanges.DislikesChange != 0 || aggregationChanges.LikesChange != 0)
			{
				var rowsAffected = await _db.VideoAggregatedReactions
					.Where(_ => _.VideoId == videoId)
					.ExecuteUpdateAsync(_ => _.SetProperty(__ => __.Likes, __ => __.Likes + aggregationChanges.LikesChange)
										.SetProperty(__ => __.Dislikes, __ => __.Dislikes + aggregationChanges.DislikesChange));
				if (rowsAffected == 0)
				{
					throw new DataAccessException("No rows were affected by the aggregation update.");
				}
			}
		}
		private async Task SetReindexingRequired(Guid videoId)
		{
			var rowsAffected = await _db.VideoSearchIndexing
					.Where(_ => _.VideoId == videoId)
					.ExecuteUpdateAsync(_ => _.SetProperty(__ => __.ReindexingRequired, __ =>  true));
			if (rowsAffected == 0)
			{
				throw new DataAccessException("No rows were affected by the aggregation update.");
			}
		}

	}
}