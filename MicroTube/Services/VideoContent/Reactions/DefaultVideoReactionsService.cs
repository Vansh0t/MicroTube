using Microsoft.Data.SqlClient;
using System.Data;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MicroTube.Services.Reactions;

namespace MicroTube.Services.VideoContent.Likes
{
	public class DefaultVideoReactionsService : IVideoReactionsService
	{
		private readonly ILogger<DefaultVideoReactionsService> _logger;
		private readonly ILikeDislikeReactionAggregator _reactionsAggregator;
		private readonly MicroTubeDbContext _db;
		public DefaultVideoReactionsService(
			ILogger<DefaultVideoReactionsService> logger,
			MicroTubeDbContext db,
			ILikeDislikeReactionAggregator reactionsAggregator)
		{
			_logger = logger;
			_db = db;
			_reactionsAggregator = reactionsAggregator;
		}

		public async Task<IServiceResult<UserVideoReaction>> SetReaction(string userId, string videoId, LikeDislikeReactionType reactionType)
		{
			if(!Guid.TryParse(userId, out var guidUserId) || !Guid.TryParse(videoId, out var guidVideoId))
			{
				return ServiceResult<UserVideoReaction>.Fail(400, "Invalid user or video id.");
			} 
			using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
			try
			{
				var reaction = await _db.UserVideoReactions.FirstOrDefaultAsync(_ => _.UserId == guidUserId && _.VideoId == guidVideoId);
				var videoReactions = await _db.VideoAggregatedReactions.FirstOrDefaultAsync(_ => _.VideoId == guidVideoId);
				if (videoReactions == null)
					throw new RequiredObjectNotFoundException($"Reactions for video with id {videoId} not found");
				LikeDislikeReactionType prevReaction;
				if (reaction == null)
				{
					reaction = new UserVideoReaction { UserId = guidUserId, VideoId = guidVideoId, ReactionType = reactionType, Time = DateTime.UtcNow };
					_db.Add(reaction);
					prevReaction = LikeDislikeReactionType.None;
				}
				else
				{
					prevReaction = reaction.ReactionType;
				}
				videoReactions = (VideoReactionsAggregation)_reactionsAggregator.UpdateReactionsAggregation(videoReactions, reactionType, prevReaction);
				reaction.ReactionType = reactionType;
				await _db.SaveChangesAsync();
				transaction.Commit();
				return ServiceResult<UserVideoReaction>.Success(reaction);
			}
			catch (RequiredObjectNotFoundException e)
			{
				transaction.Rollback();
				_logger.LogError(e, $"Failed add video like from user {userId} to video {videoId}");
				return ServiceResult<UserVideoReaction>.Fail(404, "Requested video is not found");
			}
			catch (Exception e)
			{
				transaction.Rollback();
				_logger.LogError(e, $"Failed add video like from user {userId} to video {videoId}");
				return ServiceResult<UserVideoReaction>.FailInternal();
			}
		}
		public async Task<IServiceResult<UserVideoReaction>> GetReaction(string userId, string videoId)
		{
			if (!Guid.TryParse(userId, out var guidUserId) || !Guid.TryParse(videoId, out var guidVideoId))
			{
				return ServiceResult<UserVideoReaction>.Fail(400, "Invalid user or video id.");
			}
			var reaction = await _db.UserVideoReactions.FirstOrDefaultAsync(_ => _.UserId == guidUserId && _.VideoId == guidVideoId);
			if (reaction == null)
				return ServiceResult<UserVideoReaction>.Fail(404, "Not found");
			return ServiceResult<UserVideoReaction>.Success(reaction);
		}

	}
}