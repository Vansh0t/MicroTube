using Microsoft.Extensions.Logging;
using MicroTube.Data.Access;
using MicroTube.Data.Models.Comments;
using MicroTube.Data.Models;
using MicroTube.Services.Comments.Reactions;
using MicroTube.Services.Reactions;
using MicroTube.Services.VideoContent.Comments;
using MicroTube.Tests.Mock.Models;
using MicroTube.Tests.Utils;
using Microsoft.EntityFrameworkCore;

namespace MicroTube.Tests.Unit.Comments
{
	public class DefaultVideoCommentReactionsServiceTests
	{
		[Fact]
		public async Task SetReaction_Success()
		{
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			AppUser user1 = new AppUser { Email = "email1@email.com", IsEmailConfirmed = true, PublicUsername = "user1", Username = "user1", Authentication = new MockAuthenticationData() };
			AppUser user2 = new AppUser { Email = "email2@email.com", IsEmailConfirmed = true, PublicUsername = "user2", Username = "user2", Authentication = new MockAuthenticationData() };
			Video video = new Video { ThumbnailUrls = "", Title = "Video", Urls = "" };
			VideoComment comment = new VideoComment { Content = "Some video comment", Edited = false, Deleted = false, User = user1, Video = video, Time = DateTime.UtcNow };
			VideoCommentReactionsAggregation reactions = new VideoCommentReactionsAggregation { Dislikes = 0, Likes = 0, Comment = comment, Difference = 0 };
			db.AddRange(video, user1, user2, comment, reactions);
			db.SaveChanges();
			var mockLogger = Substitute.For<ILogger<DefaultVideoCommentingService>>();
			var reactionsAggregator = new LikeDislikeReactionAggregator();

			DefaultVideoCommentReactionsService service = new DefaultVideoCommentReactionsService(db, mockLogger, reactionsAggregator);
			var commentsResult = await Task.WhenAll(service.SetReaction(user1.Id.ToString(), comment.Id.ToString(), LikeDislikeReactionType.Like),
													service.SetReaction(user2.Id.ToString(), comment.Id.ToString(), LikeDislikeReactionType.Like));
			Assert.False(commentsResult[0].IsError);
			Assert.False(commentsResult[1].IsError);
			Assert.NotNull(commentsResult[0].ResultObject);
			Assert.NotNull(commentsResult[1].ResultObject);
			var commentFromDb = db.VideoComments.Include(_ => _.Reactions).First(_ => _.Id == comment.Id);
			Assert.NotNull(commentFromDb.Reactions);
			Assert.Equal(2, commentFromDb.Reactions.Likes);
			var dislikeResult = await service.SetReaction(user1.Id.ToString(), comment.Id.ToString(), LikeDislikeReactionType.Dislike);
			Assert.False(dislikeResult.IsError);
			Assert.NotNull(dislikeResult.ResultObject);
			commentFromDb = db.VideoComments.Include(_ => _.Reactions).First(_ => _.Id == comment.Id);
			Assert.Equal(1, commentFromDb.Reactions!.Likes);
			Assert.Equal(1, commentFromDb.Reactions!.Dislikes);
		}
		[Fact]
		public async Task ReactToComment_EmailNotConfirmedFail()
		{
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			AppUser user1 = new AppUser { Email = "email1@email.com", IsEmailConfirmed = true, PublicUsername = "user1", Username = "user1", Authentication = new MockAuthenticationData() };
			AppUser user2 = new AppUser { Email = "email2@email.com", IsEmailConfirmed = false, PublicUsername = "user2", Username = "user2", Authentication = new MockAuthenticationData() };
			Video video = new Video { ThumbnailUrls = "", Title = "Video", Urls = "" };
			VideoComment comment = new VideoComment { Content = "Some video comment", Edited = false, Deleted = false, User = user1, Video = video, Time = DateTime.UtcNow };
			VideoCommentReactionsAggregation reactions = new VideoCommentReactionsAggregation { Dislikes = 0, Likes = 0, Comment = comment, Difference = 0 };
			db.AddRange(video, user1, user2, comment, reactions);
			db.SaveChanges();
			var mockLogger = Substitute.For<ILogger<DefaultVideoCommentingService>>();
			var reactionsAggregator = new LikeDislikeReactionAggregator();

			DefaultVideoCommentReactionsService service = new DefaultVideoCommentReactionsService(db, mockLogger, reactionsAggregator);
			var commentResult = await service.SetReaction(user2.Id.ToString(), comment.Id.ToString(), LikeDislikeReactionType.Like);
			Assert.True(commentResult.IsError);
			Assert.Equal(403, commentResult.Code);
		}
		[Theory]
		[InlineData(null, "3a3e152e-dcd1-4a1f-af6d-d1099a069228")]
		[InlineData("", "3a3e152e-dcd1-4a1f-af6d-d1099a069228")]
		[InlineData(" ", "3a3e152e-dcd1-4a1f-af6d-d1099a069228")]
		[InlineData("not_guid", "3a3e152e-dcd1-4a1f-af6d-d1099a069228")]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", null)]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", "")]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", " ")]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", "not_guid")]
		public async Task ReactToComment_InvalidArgumentsFail(string? userId, string? commentId)
		{
			Guid validUserId = new Guid("3a3e152e-dcd1-4a1f-af6d-d1099a069227");
			Guid validCommentId = new Guid("3a3e152e-dcd1-4a1f-af6d-d1099a069228");
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			AppUser user1 = new AppUser { Id = validUserId, Email = "email1@email.com", IsEmailConfirmed = true, PublicUsername = "user1", Username = "user1", Authentication = new MockAuthenticationData() };
			Video video = new Video { ThumbnailUrls = "", Title = "Video", Urls = "" };
			VideoComment comment = new VideoComment { Id = validCommentId, Content = "Some video comment", Edited = false, Deleted = false, User = user1, Video = video, Time = DateTime.UtcNow };
			VideoCommentReactionsAggregation reactions = new VideoCommentReactionsAggregation { Dislikes = 0, Likes = 0, Comment = comment, Difference = 0 };
			db.AddRange(video, user1, comment, reactions);
			db.SaveChanges();
			var mockLogger = Substitute.For<ILogger<DefaultVideoCommentingService>>();
			var reactionsAggregator = new LikeDislikeReactionAggregator();

			DefaultVideoCommentReactionsService service = new DefaultVideoCommentReactionsService(db, mockLogger, reactionsAggregator);
			var commentResult = await service.SetReaction(userId!, commentId!, LikeDislikeReactionType.Like);
			Assert.True(commentResult.IsError);
			Assert.Equal(400, commentResult.Code);
		}
		[Fact]
		public async Task ReactToComment_CommentDoesNotExist()
		{
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			AppUser user1 = new AppUser { Email = "email1@email.com", IsEmailConfirmed = true, PublicUsername = "user1", Username = "user1", Authentication = new MockAuthenticationData() };
			Video video = new Video { ThumbnailUrls = "", Title = "Video", Urls = "" };
			VideoComment comment = new VideoComment { Content = "Some video comment", Edited = false, Deleted = false, User = user1, Video = video, Time = DateTime.UtcNow };
			VideoCommentReactionsAggregation reactions = new VideoCommentReactionsAggregation { Dislikes = 0, Likes = 0, Comment = comment, Difference = 0 };
			db.AddRange(video, user1, comment, reactions);
			db.SaveChanges();
			var mockLogger = Substitute.For<ILogger<DefaultVideoCommentingService>>();
			var reactionsAggregator = new LikeDislikeReactionAggregator();

			DefaultVideoCommentReactionsService service = new DefaultVideoCommentReactionsService(db, mockLogger, reactionsAggregator);
			var commentResult = await service.SetReaction(user1.Id.ToString(), "3a3e152e-1111-4a1f-af6d-d1099a069227", LikeDislikeReactionType.Like);
			Assert.True(commentResult.IsError);
			Assert.Equal(404, commentResult.Code);
		}
	}
}
