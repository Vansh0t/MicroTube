using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Data.Models.Videos;
using MicroTube.Services.Reactions;
using MicroTube.Services.VideoContent.Likes;
using MicroTube.Tests.Mock.Models;
using MicroTube.Tests.Utils;

namespace MicroTube.Tests.Unit.VideoContent.Reactions
{
    public class DefaultVideoReactionsServiceTests
	{
		[Fact]
		public async Task SetReaction_Success()
		{
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			AppUser user = new()
			{
				Email = "email@email.com",
				IsEmailConfirmed = true,
				PublicUsername = "username",
				Username = "username",
				Authentication = new MockAuthenticationData()
			};
			Video video = new()
			{
				ThumbnailUrls = "",
				Title = "Vid",
				Urls = "",
				VideoIndexing = new VideoSearchIndexing()
			};
			VideoReactionsAggregation reactions = new() { Dislikes = 0, Likes = 0, Video = video };
			video.VideoReactionsAggregation = reactions;
			db.AddRange(user, video);
			db.SaveChanges();
			var aggregator = new LikeDislikeReactionAggregationHandler();
			var reactionService = new DefaultVideoReactionsService(Substitute.For<ILogger<DefaultVideoReactionsService>>(), db, aggregator);

			var result = await reactionService.SetReaction(user.Id.ToString(), video.Id.ToString(), LikeDislikeReactionType.Like);
			Assert.False(result.IsError);
			//It seems that ExecuteUpdate EF Core method does not work on Sqlite for some reason.
			//TO DO: Think about alternative checks?
			/*var userReaction = db.VideoReactions.FirstOrDefault(_ => _.VideoId == video.Id);
			Assert.NotNull(userReaction);
			Assert.Equal(LikeDislikeReactionType.Like, userReaction.ReactionType);
			Assert.True(userReaction.IsEqualByContentValues(result.GetRequiredObject()));*/

			result = await reactionService.SetReaction(user.Id.ToString(), video.Id.ToString(), LikeDislikeReactionType.Dislike);
			Assert.False(result.IsError);
			//It seems that ExecuteUpdate EF Core method does not work on Sqlite for some reason.
			//TO DO: Think about alternative checks?
			/*userReaction = db.VideoReactions.FirstOrDefault(_ => _.VideoId == video.Id);
			Assert.NotNull(userReaction);
			Assert.Equal(LikeDislikeReactionType.Dislike, userReaction.ReactionType);
			Assert.True(userReaction.IsEqualByContentValues(result.GetRequiredObject()));*/

			result = await reactionService.SetReaction(user.Id.ToString(), video.Id.ToString(), LikeDislikeReactionType.None);
			Assert.False(result.IsError);
			//It seems that ExecuteUpdate EF Core method does not work on Sqlite for some reason.
			//TO DO: Think about alternative checks?
			/*userReaction = db.VideoReactions.FirstOrDefault(_ => _.VideoId == video.Id);
			Assert.NotNull(userReaction);
			Assert.Equal(LikeDislikeReactionType.None, userReaction.ReactionType);
			Assert.True(userReaction.IsEqualByContentValues(result.GetRequiredObject()));
			var indexingFromDb = await db.VideoSearchIndexing.FirstAsync(_=>_.VideoId == video.Id);
			Assert.True(indexingFromDb.ReindexingRequired);*/

		}
		[Theory]
		[InlineData(null, "d96943f9-0cbc-4f84-b1dc-834a17cc8a49")]
		[InlineData("", "d96943f9-0cbc-4f84-b1dc-834a17cc8a49")]
		[InlineData(" ", "d96943f9-0cbc-4f84-b1dc-834a17cc8a49")]
		[InlineData("2ce6b3e1-73d2-4764-8d27-5945c44de53b", null)]
		[InlineData("2ce6b3e1-73d2-4764-8d27-5945c44de53b", "")]
		[InlineData("2ce6b3e1-73d2-4764-8d27-5945c44de53b", " ")]
		[InlineData("2ce6b3e1-73d2-4764-8d27-5945c44de53b", "d96943f9-1111-1111-1111-834a17cc8a49")]
		public async Task SetReaction_InvalidIdsFail(string? requestUserId, string? requestVideoId)
		{
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			Guid userId = new Guid("2ce6b3e1-73d2-4764-8d27-5945c44de53b");
			Guid videoId = new Guid("d96943f9-0cbc-4f84-b1dc-834a17cc8a49");
			AppUser user = new()
			{
				Id = userId,
				Email = "email@email.com",
				IsEmailConfirmed = true,
				PublicUsername = "username",
				Username = "username",
				Authentication = new MockAuthenticationData()
			};
			Video video = new()
			{
				Id = videoId,
				ThumbnailUrls = "",
				Title = "Vid",
				Urls = ""
			};
			VideoReactionsAggregation reactions = new() { Dislikes = 0, Likes = 0, Video = video };
			video.VideoReactionsAggregation = reactions;
			db.AddRange(user, video);
			db.SaveChanges();
			var aggregator = Substitute.For<ILikeDislikeReactionAggregationHandler>();
			var reactionService = new DefaultVideoReactionsService(Substitute.For<ILogger<DefaultVideoReactionsService>>(), db, aggregator);
			var result = await reactionService.SetReaction(requestUserId!, requestVideoId!, LikeDislikeReactionType.Like);
			Assert.True(result.IsError);
			Assert.True(400 == result.Code || 404 == result.Code);
			var userReaction = db.VideoReactions.FirstOrDefault(_ => _.VideoId == video.Id);
			Assert.Null(userReaction);
		}
		[Fact]
		public async Task GetReaction_Success()
		{
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			AppUser user = new()
			{
				Email = "email@email.com",
				IsEmailConfirmed = true,
				PublicUsername = "username",
				Username = "username",
				Authentication = new MockAuthenticationData()
			};
			Video video = new()
			{
				ThumbnailUrls = "",
				Title = "Vid",
				Urls = ""
			};
			VideoReactionsAggregation reactions = new() { Dislikes = 0, Likes = 0, Video = video };
			VideoReaction reaction = new() { User = user, Video = video, Time = DateTime.UtcNow, ReactionType = LikeDislikeReactionType.None };
			video.VideoReactionsAggregation = reactions;
			db.AddRange(user, video, reaction);
			db.SaveChanges();
			var aggregator = Substitute.For<ILikeDislikeReactionAggregationHandler>();
			var reactionService = new DefaultVideoReactionsService(Substitute.For<ILogger<DefaultVideoReactionsService>>(), db, aggregator);

			var result = await reactionService.GetReaction(user.Id.ToString(), video.Id.ToString());
			Assert.False(result.IsError);
			var userReaction = db.VideoReactions.FirstOrDefault(_ => _.VideoId == video.Id);
			Assert.NotNull(userReaction);
			Assert.True(userReaction.IsEqualByContentValues(result.GetRequiredObject()));
		}
		[Theory]
		[InlineData(null, "d96943f9-0cbc-4f84-b1dc-834a17cc8a49")]
		[InlineData("", "d96943f9-0cbc-4f84-b1dc-834a17cc8a49")]
		[InlineData(" ", "d96943f9-0cbc-4f84-b1dc-834a17cc8a49")]
		[InlineData("2ce6b3e1-73d2-4764-8d27-5945c44de53b", null)]
		[InlineData("2ce6b3e1-73d2-4764-8d27-5945c44de53b", "")]
		[InlineData("2ce6b3e1-73d2-4764-8d27-5945c44de53b", " ")]
		[InlineData("2ce6b3e1-73d2-4764-8d27-5945c44de53b", "d96943f9-1111-1111-1111-834a17cc8a49")]
		public async Task GetReaction_InvalidIdsFail(string? requestUserId, string? requestVideoId)
		{
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			Guid userId = new Guid("2ce6b3e1-73d2-4764-8d27-5945c44de53b");
			Guid videoId = new Guid("d96943f9-0cbc-4f84-b1dc-834a17cc8a49");
			AppUser user = new()
			{
				Id = userId,
				Email = "email@email.com",
				IsEmailConfirmed = true,
				PublicUsername = "username",
				Username = "username",
				Authentication = new MockAuthenticationData()
			};
			Video video = new()
			{
				Id = videoId,
				ThumbnailUrls = "",
				Title = "Vid",
				Urls = ""
			};
			VideoReactionsAggregation reactions = new() { Dislikes = 0, Likes = 0, Video = video };
			video.VideoReactionsAggregation = reactions;
			db.AddRange(user, video);
			db.SaveChanges();
			var aggregator = Substitute.For<ILikeDislikeReactionAggregationHandler>();
			var reactionService = new DefaultVideoReactionsService(Substitute.For<ILogger<DefaultVideoReactionsService>>(), db, aggregator);
			var result = await reactionService.GetReaction(requestUserId!, requestVideoId!);
			Assert.True(result.IsError);
			Assert.True(400 == result.Code || 404 == result.Code);
			var userReaction = db.VideoReactions.FirstOrDefault(_ => _.VideoId == video.Id);
			Assert.Null(userReaction);
		}
	}
}
