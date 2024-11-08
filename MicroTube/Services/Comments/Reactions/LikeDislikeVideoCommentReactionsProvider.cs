using Microsoft.EntityFrameworkCore;
using MicroTube.Controllers.Comments.Dto;
using MicroTube.Controllers.Reactions.Dto;
using MicroTube.Data.Access;
using MicroTube.Data.Models.Comments;

namespace MicroTube.Services.Comments.Reactions
{
	public class LikeDislikeVideoCommentReactionsProvider : ICommentReactionsProvider
	{
		private readonly MicroTubeDbContext _db;
		private readonly ILogger<LikeDislikeVideoCommentReactionsProvider> _logger;

		public LikeDislikeVideoCommentReactionsProvider(MicroTubeDbContext db, ILogger<LikeDislikeVideoCommentReactionsProvider> logger)
		{
			_db = db;
			_logger = logger;
		}

		public async Task<IServiceResult<CommentDto>> ResolveReactionsAggregationForCommentDto(CommentDtoPair commentDtoPair)
		{
			try
			{
				if (commentDtoPair.Comment is VideoComment videoComment)
				{
					VideoCommentReactionsAggregation? aggregation;
					if (videoComment.Reactions != null)
					{
						aggregation = videoComment.Reactions;

					}
					else
					{
						var aggregationFromDb = await _db.VideoCommentAggregatedReactions.AsNoTracking().FirstOrDefaultAsync(_ => _.CommentId == commentDtoPair.Comment.Id);
						aggregation = aggregationFromDb;
					}
					if (aggregation == null)
					{
						return ServiceResult<CommentDto>.Fail(404, "Unable to resolve reactions aggregation for comment " + commentDtoPair.Comment.Id);
					}
					commentDtoPair.CommentDto.ReactionsAggregation = new LikeDislikeReactionsAggregationDto(
							aggregation.TargetId.ToString(),
							aggregation.Likes,
							aggregation.Dislikes,
							aggregation.Difference);
					return ServiceResult<CommentDto>.Success(commentDtoPair.CommentDto);
				}
				return ServiceResult<CommentDto>.Fail(500, $"Comment {commentDtoPair.Comment.GetType()} is not a {nameof(VideoComment)}");
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to resolve video comment reactions aggregation due to unhandled exception.");
				return ServiceResult<CommentDto>.FailInternal();
			}
		}

		public async Task<IServiceResult<IEnumerable<CommentDto>>> ResolveReactionsAggregationForCommentDto(IEnumerable<CommentDtoPair> commentDtoPairs)
		{
			try
			{
				HashSet<Guid> idsToFetch = commentDtoPairs.Where(_ => ((VideoComment)_.Comment).ReactionsAggregation == null).Select(_ => _.Id).ToHashSet();
				var aggregations = await _db.VideoCommentAggregatedReactions.Where(_ => idsToFetch.Contains(_.Id)).ToDictionaryAsync(_ => _.CommentId);
				foreach(CommentDtoPair pair in commentDtoPairs)
				{
					VideoComment? comment = pair.Comment as VideoComment;
					if(comment == null)
					{
						continue;
					}
					var aggregation = comment.ReactionsAggregation as VideoCommentReactionsAggregation;
					if(aggregation == null && !aggregations.TryGetValue(comment.Id, out aggregation))
					{
						continue;
					}
					pair.CommentDto.ReactionsAggregation = new LikeDislikeReactionsAggregationDto
					(
						comment.Id.ToString(),
						aggregation.Likes,
						aggregation.Dislikes,
						aggregation.Difference
					);
				}
				return ServiceResult<IEnumerable<CommentDto>>.Success(commentDtoPairs.Select(_=>_.CommentDto));
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to resolve video comment reactions aggregation due to unhandled exception.");
				return ServiceResult<IEnumerable<CommentDto>>.FailInternal();
			}
		}

		public async Task<IServiceResult<CommentDto>> ResolveUserReactionForCommentDto(string userId, CommentDtoPair commentDtoPair)
		{
			try
			{
				if (commentDtoPair.Comment is VideoComment videoComment)
				{
					VideoCommentReaction? userReaction = await _db.VideoCommentReactions.AsNoTracking().FirstOrDefaultAsync(_ => _.CommentId == commentDtoPair.Comment.Id);
					if (userReaction == null)
					{
						return ServiceResult<CommentDto>.Success(commentDtoPair.CommentDto);
					}
					commentDtoPair.CommentDto.Reaction = new LikeDislikeReactionDto { UserId = userId, ReactionType = userReaction.ReactionType, TargetId = userReaction.CommentId.ToString(), Time = userReaction.Time };
					return ServiceResult<CommentDto>.Success(commentDtoPair.CommentDto);
				}
				return ServiceResult<CommentDto>.Fail(500, $"Comment {commentDtoPair.Comment.GetType()} is not a {nameof(VideoComment)}");
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to resolve video comment user reaction due to unhandled exception.");
				return ServiceResult<CommentDto>.FailInternal();
			}

		}

		public async Task<IServiceResult<IEnumerable<CommentDto>>> ResolveUserReactionForCommentDto(string userId, IEnumerable<CommentDtoPair> commentDtoPairs)
		{
			try
			{
				if(!Guid.TryParse(userId, out var guidUserId))
				{
					throw new ArgumentException("Invalid userId");
				}
				HashSet<Guid> idsToFetch = commentDtoPairs.Select(_ => _.Id).ToHashSet();
				var userReactions = await _db.VideoCommentReactions.Where(_ => _.UserId == guidUserId && idsToFetch.Contains(_.CommentId)).ToDictionaryAsync(_ => _.CommentId);
				foreach(var pair in commentDtoPairs)
				{
					if(!userReactions.TryGetValue(pair.Id, out var commentReaction))
					{
						continue;
					}
					pair.CommentDto.Reaction = new LikeDislikeReactionDto 
					{
						UserId = commentReaction.UserId.ToString(),
						ReactionType = commentReaction.ReactionType,
						TargetId = commentReaction.CommentId.ToString(),
						Time = commentReaction.Time
					};
				}
				return ServiceResult<IEnumerable<CommentDto>>.Success(commentDtoPairs.Select(_=>_.CommentDto));
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to resolve video comment user reaction due to unhandled exception.");
				return ServiceResult<IEnumerable<CommentDto>>.FailInternal();
			}
		}
	}
}
