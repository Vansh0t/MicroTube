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

		public async Task<IServiceResult<ILikeDislikeReaction>> SetReaction(string userId, string videoId, LikeDislikeReactionType reactionType)
		{
			if(!Guid.TryParse(userId, out var guidUserId) || !Guid.TryParse(videoId, out var guidVideoId))
			{
				return ServiceResult<ILikeDislikeReaction>.Fail(400, "Invalid user or video id.");
			} 
			using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
			try
			{
				VideoReactionsAggregation reactionsAggregation = await GetReactionsAggregation(guidVideoId);
				(VideoReaction reaction, bool created) = await GetOrCreateReaction(guidUserId, guidVideoId);
				if (created)
				{
					_db.Add(reaction);
				}
				else if(reaction.ReactionType == reactionType)
				{
					return ServiceResult<ILikeDislikeReaction>.Success(reaction);
				}
				reactionsAggregation = (VideoReactionsAggregation)_reactionsAggregator.UpdateReactionsAggregation(reactionsAggregation, reactionType, reaction.ReactionType);
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
		private async Task<VideoReactionsAggregation> GetReactionsAggregation(Guid videoId)
		{
			VideoReactionsAggregation? reactionsAggregation = await _db.VideoAggregatedReactions.FirstOrDefaultAsync(_ => _.VideoId == videoId);
			if (reactionsAggregation == null)
				throw new RequiredObjectNotFoundException($"Reactions for video with id {videoId} not found");
			return reactionsAggregation;
		}
		private async Task<VideoSearchIndexing> SetReindexingRequired(Guid videoId)
		{
			VideoSearchIndexing videoIndexing = await _db.VideoSearchIndexing.FirstAsync(_ => _.VideoId == videoId);
			videoIndexing.ReindexingRequired = true;
			return videoIndexing;
		}
	}
}