using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Data.Models.Comments;
using MicroTube.Data.Models.Videos;
using MicroTube.Services;
using MicroTube.Services.Comments;
using MicroTube.Services.Comments.Reactions;
using MicroTube.Services.Reactions;
using MicroTube.Services.Validation;
using MicroTube.Tests.Mock.Models;
using MicroTube.Tests.Utils;

namespace MicroTube.Tests.Unit.Comments
{
    public class DefaultVideoCommentingServiceTests
	{
		[Fact]
		public async Task Comment_Success()
		{
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			AppUser user = new AppUser { Email = "email@email.com", IsEmailConfirmed = true, PublicUsername = "user", Username = "user", Authentication = new MockAuthenticationData() };
			Video video = new Video { ThumbnailUrls = "", Title = "Video", Urls = "" };
			db.AddRange(video, user);
			db.SaveChanges();
			var mockContentValidator = Substitute.For<ICommentContentValidator>();
			mockContentValidator.Validate("").ReturnsForAnyArgs(ServiceResult.Success());
			var mockLogger = Substitute.For<ILogger<DefaultVideoCommentingService>>();
			var mockReactionsAggregator = Substitute.For<ILikeDislikeReactionAggregationHandler>();
			DefaultVideoCommentingService service = new DefaultVideoCommentingService(db, mockLogger, mockContentValidator, mockReactionsAggregator);
			string commentContent = "Some comment content";
			var commentResult = await service.Comment(user.Id.ToString(), video.Id.ToString(), commentContent);
			Assert.False(commentResult.IsError);
			Assert.NotNull(commentResult.ResultObject);
			var commentFromResult = commentResult.GetRequiredObject();
			Assert.Equal(commentContent, commentFromResult.Content);
			Assert.False(commentFromResult.Deleted);
			Assert.False(commentFromResult.Edited);
			var commentFromDb = db.VideoComments.Include(_ => _.CommentReactionsAggregation).Include(_=>_.Video).First(_ => _.Id == commentFromResult.Id);
			Assert.True(commentFromDb.IsEqualByContentValues(commentFromResult));
			Assert.NotNull(commentFromDb.CommentReactionsAggregation);
			Assert.NotNull(commentFromDb.Video);
			Assert.Equal(0, commentFromDb.CommentReactionsAggregation.Likes);
			Assert.Equal(0, commentFromDb.CommentReactionsAggregation.Dislikes);
			Assert.Equal(1, commentFromDb.Video.CommentsCount);
		}
		[Fact]
		public async Task Comment_EmailConfirmationFail()
		{
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			AppUser user = new AppUser { Email = "email@email.com", IsEmailConfirmed = false, PublicUsername = "user", Username = "user", Authentication = new MockAuthenticationData() };
			Video video = new Video { ThumbnailUrls = "", Title = "Video", Urls = "" };
			db.AddRange(video, user);
			db.SaveChanges();
			var mockContentValidator = Substitute.For<ICommentContentValidator>();
			mockContentValidator.Validate("").ReturnsForAnyArgs(ServiceResult.Success());
			var mockLogger = Substitute.For<ILogger<DefaultVideoCommentingService>>();
			var mockReactionsAggregator = Substitute.For<ILikeDislikeReactionAggregationHandler>();
			DefaultVideoCommentingService service = new DefaultVideoCommentingService(db, mockLogger, mockContentValidator, mockReactionsAggregator);
			string commentContent = "Some comment content";
			var commentResult = await service.Comment(user.Id.ToString(), video.Id.ToString(), commentContent);
			Assert.True(commentResult.IsError);
			Assert.Equal(403, commentResult.Code);
		}
		[Theory]
		[InlineData(null, "3a3e152e-dcd1-4a1f-af6d-d1099a069228", "Some comment content")]
		[InlineData("", "3a3e152e-dcd1-4a1f-af6d-d1099a069228", "Some comment content")]
		[InlineData(" ", "3a3e152e-dcd1-4a1f-af6d-d1099a069228", "Some comment content")]
		[InlineData("not_guid", "3a3e152e-dcd1-4a1f-af6d-d1099a069228", "Some comment content")]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", null, "Some comment content")]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", "", "Some comment content")]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", " ", "Some comment content")]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", "not_guid", "Some comment content")]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", "3a3e152e-dcd1-4a1f-af6d-d1099a069228", null)]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", "3a3e152e-dcd1-4a1f-af6d-d1099a069228", "")]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", "3a3e152e-dcd1-4a1f-af6d-d1099a069228", " ")]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", "3a3e152e-dcd1-4a1f-af6d-d1099a069228", "Invalid content")]
		public async Task Comment_InvalidArgumentsFail(string? userId, string? videoId, string? content)
		{
			Guid validUserId =new Guid("3a3e152e-dcd1-4a1f-af6d-d1099a069227");
			Guid validVideoId = new Guid("3a3e152e-dcd1-4a1f-af6d-d1099a069228");
			string validContent = "Some comment content";
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			AppUser user = new AppUser { Id = validUserId, Email = "email@email.com", IsEmailConfirmed = false, PublicUsername = "user", Username = "user", Authentication = new MockAuthenticationData() };
			Video video = new Video { Id = validVideoId, ThumbnailUrls = "", Title = "Video", Urls = "" };
			db.AddRange(video, user);
			db.SaveChanges();
			var mockContentValidator = Substitute.For<ICommentContentValidator>();
			mockContentValidator.Validate(Arg.Is<string>(_=> _ !=validContent)).Returns(ServiceResult.Fail(400, "Bad content"));
			mockContentValidator.Validate(Arg.Is<string>(_ => _ == validContent)).Returns(ServiceResult.Success());
			var mockLogger = Substitute.For<ILogger<DefaultVideoCommentingService>>();
			var mockReactionsAggregator = Substitute.For<ILikeDislikeReactionAggregationHandler>();
			DefaultVideoCommentingService service = new DefaultVideoCommentingService(db, mockLogger, mockContentValidator, mockReactionsAggregator);
			var commentResult = await service.Comment(userId!, videoId!, content!);
			Assert.True(commentResult.IsError);
			Assert.Equal(400, commentResult.Code);
		}
		[Theory]
		[InlineData("3a3e152e-1111-4a1f-af6d-d1099a069227", "3a3e152e-dcd1-4a1f-af6d-d1099a069228")]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", "3a3e152e-1111-4a1f-af6d-d1099a069228")]
		public async Task Comment_UserOrVideoDoNotExistFail(string userId, string videoId)
		{
			Guid validUserId = new Guid("3a3e152e-dcd1-4a1f-af6d-d1099a069227");
			Guid validVideoId = new Guid("3a3e152e-dcd1-4a1f-af6d-d1099a069228");
			string validContent = "Some comment content";
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			AppUser user = new AppUser { Id = validUserId, Email = "email@email.com", IsEmailConfirmed = false, PublicUsername = "user", Username = "user", Authentication = new MockAuthenticationData() };
			Video video = new Video { Id = validVideoId, ThumbnailUrls = "", Title = "Video", Urls = "" };
			db.AddRange(video, user);
			db.SaveChanges();
			var mockContentValidator = Substitute.For<ICommentContentValidator>();
			mockContentValidator.Validate(Arg.Is<string>(_ => _ != validContent)).Returns(ServiceResult.Fail(400, "Bad content"));
			mockContentValidator.Validate(Arg.Is<string>(_ => _ == validContent)).Returns(ServiceResult.Success());
			var mockLogger = Substitute.For<ILogger<DefaultVideoCommentingService>>();
			var mockReactionsAggregator = Substitute.For<ILikeDislikeReactionAggregationHandler>();
			DefaultVideoCommentingService service = new DefaultVideoCommentingService(db, mockLogger, mockContentValidator, mockReactionsAggregator);
			var commentResult = await service.Comment(userId!, videoId!, validContent);
			Assert.True(commentResult.IsError);
			Assert.Equal(404, commentResult.Code);
		}
		[Fact]
		public async Task EditComment_Success()
		{
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			AppUser user = new AppUser { Email = "email@email.com", IsEmailConfirmed = true, PublicUsername = "user", Username = "user", Authentication = new MockAuthenticationData() };
			Video video = new Video { ThumbnailUrls = "", Title = "Video", Urls = "" };
			string editedContent = "Edited comment content";
			VideoComment comment = new VideoComment { Content = "Some video comment", Edited = false, Deleted = false, User = user, Video = video, Time = DateTime.UtcNow };
			db.AddRange(video, user, comment);
			db.SaveChanges();
			var mockContentValidator = Substitute.For<ICommentContentValidator>();
			mockContentValidator.Validate("").ReturnsForAnyArgs(ServiceResult.Success());
			var mockLogger = Substitute.For<ILogger<DefaultVideoCommentingService>>();
			var mockReactionsAggregator = Substitute.For<ILikeDislikeReactionAggregationHandler>();
			DefaultVideoCommentingService service = new DefaultVideoCommentingService(db, mockLogger, mockContentValidator, mockReactionsAggregator);
			var commentResult = await service.EditComment(user.Id.ToString(), editedContent, comment.Id.ToString());
			Assert.False(commentResult.IsError);
			Assert.NotNull(commentResult.ResultObject);
			var commentFromResult = commentResult.GetRequiredObject();
			Assert.Equal(editedContent, commentFromResult.Content);
			Assert.False(commentFromResult.Deleted);
			Assert.True(commentFromResult.Edited);
			var commentFromDb = db.VideoComments.First(_ => _.Id == commentFromResult.Id);
			Assert.Equal(editedContent, commentFromDb.Content);
			Assert.False(commentFromDb.Deleted);
			Assert.True(commentFromDb.Edited);
		}
		[Fact]
		public async Task EditComment_CommentDoesNotExistFail()
		{
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			AppUser user = new AppUser { Email = "email@email.com", IsEmailConfirmed = true, PublicUsername = "user", Username = "user", Authentication = new MockAuthenticationData() };
			Video video = new Video { ThumbnailUrls = "", Title = "Video", Urls = "" };
			string editedContent = "Edited comment content";
			db.AddRange(video, user);
			db.SaveChanges();
			var mockContentValidator = Substitute.For<ICommentContentValidator>();
			mockContentValidator.Validate("").ReturnsForAnyArgs(ServiceResult.Success());
			var mockLogger = Substitute.For<ILogger<DefaultVideoCommentingService>>();
			var mockReactionsAggregator = Substitute.For<ILikeDislikeReactionAggregationHandler>();
			DefaultVideoCommentingService service = new DefaultVideoCommentingService(db, mockLogger, mockContentValidator, mockReactionsAggregator);
			var commentResult = await service.EditComment(user.Id.ToString(), editedContent, "3a3e152e-dcd1-4a1f-af6d-d1099a069227");
			Assert.True(commentResult.IsError);
			Assert.Equal(404, commentResult.Code);
		}
		[Theory]
		[InlineData(null, "Some edited content", "3a3e152e-dcd1-4a1f-af6d-d1099a069228")]
		[InlineData("", "Some edited content", "3a3e152e-dcd1-4a1f-af6d-d1099a069228")]
		[InlineData(" ", "Some edited content", "3a3e152e-dcd1-4a1f-af6d-d1099a069228")]
		[InlineData("not_guid", "Some edited content", "3a3e152e-dcd1-4a1f-af6d-d1099a069228")]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", null, "3a3e152e-dcd1-4a1f-af6d-d1099a069228")]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", "", "3a3e152e-dcd1-4a1f-af6d-d1099a069228")]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", " ", "3a3e152e-dcd1-4a1f-af6d-d1099a069228")]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", "Invalid content", "3a3e152e-dcd1-4a1f-af6d-d1099a069228")]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", "Some edited content", null)]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", "Some edited content", "")]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", "Some edited content", " ")]
		[InlineData("3a3e152e-dcd1-4a1f-af6d-d1099a069227", "Some edited content", "not_guid")]
		public async Task EditComment_InvalidArgumentsFail(string? userId, string? editedContent, string? commentId)
		{
			Guid validUserId = new Guid("3a3e152e-dcd1-4a1f-af6d-d1099a069227");
			Guid validCommentId = new Guid("3a3e152e-dcd1-4a1f-af6d-d1099a069228");
			string validEditedContent = "Some edited content";
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			AppUser user = new AppUser { Id = validUserId, Email = "email@email.com", IsEmailConfirmed = true, PublicUsername = "user", Username = "user", Authentication = new MockAuthenticationData() };
			Video video = new Video { ThumbnailUrls = "", Title = "Video", Urls = "" };
			VideoComment comment = new VideoComment {Id=validCommentId, Content = "Does not matter", Deleted = false, Edited = false, User = user, Video = video, Time = DateTime.UtcNow };
			db.AddRange(video, user, comment);
			db.SaveChanges();
			var mockContentValidator = Substitute.For<ICommentContentValidator>();
			mockContentValidator.Validate(Arg.Is<string>(_ => _ != validEditedContent)).Returns(ServiceResult.Fail(400, "Bad content"));
			mockContentValidator.Validate(Arg.Is<string>(_ => _ == validEditedContent)).Returns(ServiceResult.Success());
			var mockLogger = Substitute.For<ILogger<DefaultVideoCommentingService>>();
			var mockReactionsAggregator = Substitute.For<ILikeDislikeReactionAggregationHandler>();
			DefaultVideoCommentingService service = new DefaultVideoCommentingService(db, mockLogger, mockContentValidator, mockReactionsAggregator);
			var commentResult = await service.EditComment(userId!, editedContent!, commentId!);
			Assert.True(commentResult.IsError);
			Assert.Equal(400, commentResult.Code);
		}
		[Fact]
		public async Task EditComment_NotCommentOwnerFail()
		{
			Guid impostorId = new Guid("3a3e152e-1111-4a1f-af6d-d1099a069227");
			Guid validUserId = new Guid("3a3e152e-dcd1-4a1f-af6d-d1099a069227");
			Guid validCommentId = new Guid("3a3e152e-dcd1-4a1f-af6d-d1099a069228");
			string validEditedContent = "Some edited content";
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			AppUser user = new AppUser { Id = validUserId, Email = "email1@email.com", IsEmailConfirmed = true, PublicUsername = "user1", Username = "user1", Authentication = new MockAuthenticationData() };
			AppUser impostorUser = new AppUser { Id = impostorId, Email = "email2@email.com", IsEmailConfirmed = true, PublicUsername = "user2", Username = "user2", Authentication = new MockAuthenticationData() };
			Video video = new Video { ThumbnailUrls = "", Title = "Video", Urls = "" };
			VideoComment comment = new VideoComment {Id = validCommentId, Content = "Does not matter", Deleted = false, Edited = false, User = user, Video = video, Time = DateTime.UtcNow };
			db.AddRange(video, user, impostorUser, comment);
			db.SaveChanges();
			var mockContentValidator = Substitute.For<ICommentContentValidator>();
			mockContentValidator.Validate(Arg.Is<string>(_ => _ != validEditedContent)).Returns(ServiceResult.Fail(400, "Bad content"));
			mockContentValidator.Validate(Arg.Is<string>(_ => _ == validEditedContent)).Returns(ServiceResult.Success());
			var mockLogger = Substitute.For<ILogger<DefaultVideoCommentingService>>();
			var mockReactionsAggregator = Substitute.For<ILikeDislikeReactionAggregationHandler>();
			DefaultVideoCommentingService service = new DefaultVideoCommentingService(db, mockLogger, mockContentValidator, mockReactionsAggregator);
			var commentResult = await service.EditComment(impostorId.ToString(), validEditedContent, validCommentId.ToString());
			Assert.True(commentResult.IsError);
			Assert.Equal(403, commentResult.Code);
		}
		[Fact]
		public async Task DeleteComment_Success()
		{
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			AppUser user = new AppUser { Email = "email@email.com", IsEmailConfirmed = true, PublicUsername = "user", Username = "user", Authentication = new MockAuthenticationData() };
			Video video = new Video { ThumbnailUrls = "", Title = "Video", Urls = "", CommentsCount = 1 };
			VideoComment comment = new VideoComment { Content = "Some video comment", Edited = false, Deleted = false, User = user, Video = video, Time = DateTime.UtcNow };
			db.AddRange(video, user, comment);
			db.SaveChanges();
			var mockContentValidator = Substitute.For<ICommentContentValidator>();
			mockContentValidator.Validate("").ReturnsForAnyArgs(ServiceResult.Success());
			var mockLogger = Substitute.For<ILogger<DefaultVideoCommentingService>>();
			var mockReactionsAggregator = Substitute.For<ILikeDislikeReactionAggregationHandler>();
			DefaultVideoCommentingService service = new DefaultVideoCommentingService(db, mockLogger, mockContentValidator, mockReactionsAggregator);
			var deleteResult = await service.DeleteComment(user.Id.ToString(), comment.Id.ToString());
			Assert.False(deleteResult.IsError);
			var commentFromDb = await db.VideoComments.Include(_=>_.Video).FirstOrDefaultAsync(_ => _.Id == comment.Id);
			Assert.NotNull(commentFromDb);
			Assert.True(commentFromDb.Deleted);
			Assert.Equal(0, commentFromDb.Video!.CommentsCount);
		}
		[Fact]
		public async Task DeleteComment_NotOwnerFail()
		{
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			AppUser user = new AppUser { Email = "email@email.com", IsEmailConfirmed = true, PublicUsername = "user", Username = "user", Authentication = new MockAuthenticationData() };
			AppUser impostorUser = new AppUser { Email = "email2@email.com", IsEmailConfirmed = true, PublicUsername = "user2", Username = "user2", Authentication = new MockAuthenticationData() };
			Video video = new Video { ThumbnailUrls = "", Title = "Video", Urls = "", CommentsCount = 1 };
			VideoComment comment = new VideoComment { Content = "Some video comment", Edited = false, Deleted = false, User = user, Video = video, Time = DateTime.UtcNow };
			db.AddRange(video, user, impostorUser, comment);
			db.SaveChanges();
			var mockContentValidator = Substitute.For<ICommentContentValidator>();
			mockContentValidator.Validate("").ReturnsForAnyArgs(ServiceResult.Success());
			var mockLogger = Substitute.For<ILogger<DefaultVideoCommentingService>>();
			var mockReactionsAggregator = Substitute.For<ILikeDislikeReactionAggregationHandler>();
			DefaultVideoCommentingService service = new DefaultVideoCommentingService(db, mockLogger, mockContentValidator, mockReactionsAggregator);
			var deleteResult = await service.DeleteComment(impostorUser.Id.ToString(), comment.Id.ToString());
			Assert.True(deleteResult.IsError);
			Assert.Equal(403, deleteResult.Code);
			var commentFromDb = await db.VideoComments.Include(_=>_.Video).FirstOrDefaultAsync(_ => _.Id == comment.Id);
			Assert.NotNull(commentFromDb);
			Assert.False(commentFromDb.Deleted);
			Assert.Equal(1, commentFromDb.Video!.CommentsCount);
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
		public async Task DeleteComment_InvalidArgumentsFail(string? userId, string? commentId)
		{
			Guid validUserId = new Guid("3a3e152e-dcd1-4a1f-af6d-d1099a069227");
			Guid validCommentId = new Guid("3a3e152e-dcd1-4a1f-af6d-d1099a069228");
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			AppUser user = new AppUser {Id = validUserId, Email = "email@email.com", IsEmailConfirmed = true, PublicUsername = "user", Username = "user", Authentication = new MockAuthenticationData() };
			Video video = new Video { ThumbnailUrls = "", Title = "Video", Urls = "", CommentsCount = 1 };
			VideoComment comment = new VideoComment {Id = validCommentId, Content = "Some video comment", Edited = false, Deleted = false, User = user, Video = video, Time = DateTime.UtcNow };
			db.AddRange(video, user, comment);
			db.SaveChanges();
			var mockContentValidator = Substitute.For<ICommentContentValidator>();
			mockContentValidator.Validate("").ReturnsForAnyArgs(ServiceResult.Success());
			var mockLogger = Substitute.For<ILogger<DefaultVideoCommentingService>>();
			var mockReactionsAggregator = Substitute.For<ILikeDislikeReactionAggregationHandler>();
			DefaultVideoCommentingService service = new DefaultVideoCommentingService(db, mockLogger, mockContentValidator, mockReactionsAggregator);
			var deleteResult = await service.DeleteComment(userId!, commentId!);
			Assert.True(deleteResult.IsError);
			Assert.Equal(400, deleteResult.Code);
		}
		[Fact]
		public async Task DeleteComment_CommentDoesNotExistFail()
		{
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			AppUser user = new AppUser { Email = "email@email.com", IsEmailConfirmed = true, PublicUsername = "user", Username = "user", Authentication = new MockAuthenticationData() };
			Video video = new Video { ThumbnailUrls = "", Title = "Video", Urls = "", CommentsCount = 1 };
			VideoComment comment = new VideoComment { Content = "Some video comment", Edited = false, Deleted = false, User = user, Video = video, Time = DateTime.UtcNow };
			db.AddRange(video, user, comment);
			db.SaveChanges();
			var mockContentValidator = Substitute.For<ICommentContentValidator>();
			mockContentValidator.Validate("").ReturnsForAnyArgs(ServiceResult.Success());
			var mockLogger = Substitute.For<ILogger<DefaultVideoCommentingService>>();
			var mockReactionsAggregator = Substitute.For<ILikeDislikeReactionAggregationHandler>();
			DefaultVideoCommentingService service = new DefaultVideoCommentingService(db, mockLogger, mockContentValidator, mockReactionsAggregator);
			var deleteResult = await service.DeleteComment(user.Id.ToString(), "3a3e152e-dcd1-4a1f-af6d-d1099a069227");
			Assert.True(deleteResult.IsError);
			Assert.Equal(404, deleteResult.Code);
		}
	}
}
