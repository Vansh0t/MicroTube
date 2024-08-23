using Microsoft.Data.SqlClient;
using System.Data;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MicroTube.Services.VideoContent.Reactions;

namespace MicroTube.Services.VideoContent.Likes
{
	public class DefaultVideoReactionsService : IVideoReactionsService
	{
		private readonly ILogger<DefaultVideoReactionsService> _logger;
		private readonly IVideoReactionsAggregator _reactionsAggregator;
		private readonly MicroTubeDbContext _db;
		public DefaultVideoReactionsService(
			ILogger<DefaultVideoReactionsService> logger,
			MicroTubeDbContext db,
			IVideoReactionsAggregator reactionsAggregator)
		{
			_logger = logger;
			_db = db;
			_reactionsAggregator = reactionsAggregator;
		}

		public async Task<IServiceResult<UserVideoReaction>> SetReaction(string userId, string videoId, ReactionType reactionType)
		{
			IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
			try
			{
				var reaction = await _db.UserVideoReactions.FirstOrDefaultAsync(_ => _.UserId == new Guid(userId) && _.VideoId == new Guid(videoId));
				var videoReactions = await _db.VideoReactions.FirstOrDefaultAsync(_ => _.VideoId == new Guid(videoId));
				if (videoReactions == null)
					throw new RequiredObjectNotFoundException($"Reactions for video with id {videoId} not found");
				ReactionType prevReaction;
				if (reaction == null)
				{
					reaction = new UserVideoReaction { UserId = new Guid(userId), VideoId = new Guid(videoId), ReactionType = reactionType, Time = DateTime.UtcNow };
					_db.Add(reaction);
					prevReaction = ReactionType.None;
				}
				else
				{
					prevReaction = reaction.ReactionType;
				}
				videoReactions = _reactionsAggregator.UpdateReactionsAggregation(videoReactions, reactionType, prevReaction);
				reaction.ReactionType = reactionType;
				await _db.SaveChangesAsync();
				transaction.Commit();
				return ServiceResult<UserVideoReaction>.Success(reaction);
			}
			catch (SqlException e)
			{
				//unique key constraint violation
				transaction.Rollback();
				if (e.Number == 2627)
				{
					return ServiceResult<UserVideoReaction>.Fail(400, "Already liked.");
				}
				_logger.LogError(e, $"Failed add video like from user {userId} to video {videoId}");
				return ServiceResult<UserVideoReaction>.FailInternal();
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
			var reaction = await _db.UserVideoReactions.FirstOrDefaultAsync(_ => _.UserId == new Guid(userId) && _.VideoId == new Guid(videoId));
			if (reaction == null)
				return ServiceResult<UserVideoReaction>.Fail(404, "Not found");
			return ServiceResult<UserVideoReaction>.Success(reaction);
		}

	}
}