using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MicroTube.Data.Access;
using MicroTube.Data.Models.Comments;
using MicroTube.Services.Comments;
using MicroTube.Services.Reactions;
using MicroTube.Services.Validation;
using System.Data;

namespace MicroTube.Services.VideoContent.Comments
{
    public class DefaultVideoCommentingService : ICommentingService
    {
        private readonly MicroTubeDbContext _db;
        private readonly ILogger<DefaultVideoCommentingService> _logger;
        private readonly ICommentContentValidator _contentValidator;
        private readonly ILikeDislikeReactionAggregator _reactionAggregator;

        public DefaultVideoCommentingService(
            MicroTubeDbContext db,
            ILogger<DefaultVideoCommentingService> logger,
            ICommentContentValidator contentValidator,
            ILikeDislikeReactionAggregator reactionAggregator)
        {
            _db = db;
            _logger = logger;
            _contentValidator = contentValidator;
            _reactionAggregator = reactionAggregator;
        }

        public async Task<IServiceResult<IComment>> Comment(string userId, string videoId, string content)
        {
            var contentValidationResult = _contentValidator.Validate(content);
            if (contentValidationResult.IsError)
            {
                return ServiceResult<IComment>.Fail(contentValidationResult.Code, contentValidationResult.Error!);
            }
            if (!Guid.TryParse(userId, out var guidUserId) || !Guid.TryParse(videoId, out var guidVideoId))
            {
                return ServiceResult<IComment>.Fail(400, "Invalid user id or video id");
            }
            var video = await _db.Videos.FirstOrDefaultAsync(_ => _.Id == guidVideoId);
            var user = await _db.Users.FirstOrDefaultAsync(_ => _.Id == guidUserId);
            if (video == null)
            {
                return ServiceResult<IComment>.Fail(404, "Target video does not exist");
            }
            if (user == null)
            {
                return ServiceResult<IComment>.Fail(404, "User does not exist");
            }
            if (!user.IsEmailConfirmed)
            {
                return ServiceResult<IComment>.Fail(403, "Email confirmation is required for this action.");
            }
            using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var comment = new VideoComment
                {
                    Content = content,
                    Reactions = new VideoCommentReactionsAggregation { Dislikes = 0, Likes = 0, Difference = 0 },
                    UserId = guidUserId,
                    User = user,
                    VideoId = guidVideoId,
                    Video = video,
                    Time = DateTime.UtcNow
                };
                comment.Reactions.Comment = comment;
                video.CommentsCount++;
                _db.Add(comment);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return ServiceResult<IComment>.Success(comment);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e, $"Failed to create a comment. User: {userId}, Target: {videoId}");
                return ServiceResult<IComment>.FailInternal();
            }
        }
        public async Task<IServiceResult<IComment>> EditComment(string userId, string newContent, string commentId)
        {
            var contentValidationResult = _contentValidator.Validate(newContent);
            if (contentValidationResult.IsError)
            {
                return ServiceResult<IComment>.Fail(contentValidationResult.Code, contentValidationResult.Error!);
            }
            if (!Guid.TryParse(commentId, out Guid guidCommentId) || !Guid.TryParse(userId, out var guidUserId))
            {
                return ServiceResult<IComment>.Fail(400, "Invalid comment id.");
            }
            try
            {
                VideoComment? comment = await _db.VideoComments.FirstOrDefaultAsync(_ => _.Id == guidCommentId && !_.Deleted);
                if (comment == null)
                {
                    return ServiceResult<IComment>.Fail(404, "Comment does not exist");
                }
                if (comment.UserId != guidUserId)
                {
                    _logger.LogWarning($"User {userId} tried to update not owned comment {commentId}");
                    return ServiceResult<IComment>.Fail(403, "Forbidden.");
                }
                comment.Content = newContent;
                comment.Edited = true;
                await _db.SaveChangesAsync();
                return ServiceResult<IComment>.Success(comment);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to update a comment. Comment: {commentId}");
                return ServiceResult<IComment>.FailInternal();
            }
        }
        public async Task<IServiceResult<IComment>> DeleteComment(string userId, string commentId)
        {
            if (!Guid.TryParse(commentId, out Guid guidCommentId) || !Guid.TryParse(userId, out var guidUserId))
            {
                return ServiceResult<IComment>.Fail(400, "Invalid comment id.");
            }
            using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
            try
            {
                VideoComment? comment = await _db.VideoComments.Include(_ => _.Video).FirstOrDefaultAsync(_ => _.Id == guidCommentId);
                if (comment == null)
                {
                    return ServiceResult<IComment>.Fail(404, "Comment does not exist");
                }
                if (comment.UserId != guidUserId)
                {
                    _logger.LogWarning($"User {userId} tried to delete not owned comment {commentId}");
                    return ServiceResult<IComment>.Fail(403, "Forbidden.");
                }
                comment.Deleted = true;
                if (comment.Video != null)
                {
                    comment.Video.CommentsCount--;
                }
                _db.SaveChanges();
                await transaction.CommitAsync();
                return ServiceResult<IComment>.Success(comment);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e, $"Failed to delete a comment. Comment: {commentId}");
                return ServiceResult<IComment>.FailInternal();
            }
        }
    }
}
