using Microsoft.Extensions.Logging;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.VideoContent.Views;
using MicroTube.Tests.Utils;
using NSubstitute;
using System.Net;

namespace MicroTube.Tests.Unit.VideoContent
{
	public class DefaultVideoViewsAggregatorServiceTests
	{
		[Fact]
		public async Task CreateViewForAggregation_Success()
		{
			IPAddress ip = IPAddress.Loopback;
			AppUser user = new AppUser
			{
				Email = "email@email.com",
				IsEmailConfirmed = true,
				PublicUsername = "username",
				Username = "username"
			};
			Video video = new Video { ThumbnailUrls = "", Title = "Vid", Urls = "" };
			VideoViewsAggregation views = new VideoViewsAggregation { Views = 99, Video = video };
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			db.AddRange(user, video, views);
			db.SaveChanges();
			var aggregator = new DefaultVideoViewsAggregatorService(Substitute.For<ILogger<DefaultVideoViewsAggregatorService>>(), db);
			var result = await aggregator.CreateViewForAggregation(video.Id.ToString(), ip.ToString());
			Assert.False(result.IsError);
			Assert.Equal(202, result.Code);
			var viewFromDb = db.VideoViews.Where(_ => _.VideoId == video.Id && _.Ip == ip.ToString());
			Assert.Single(viewFromDb);
			result = await aggregator.CreateViewForAggregation(video.Id.ToString(), ip.ToString());
			Assert.False(result.IsError);
			Assert.Equal(202, result.Code);
			viewFromDb = db.VideoViews.Where(_ => _.VideoId == video.Id && _.Ip == ip.ToString());
			Assert.Single(viewFromDb);
		}
		[Theory]
		[InlineData(null, "127.0.0.1")]
		[InlineData("", "127.0.0.1")]
		[InlineData(" ", "127.0.0.1")]
		[InlineData("1aeb8378-1111-1111-8d72-570907a948fa", "127.0.0.1")]
		[InlineData("1aeb8378-a102-4a5f-8d72-570907a948fa", null)]
		[InlineData("1aeb8378-a102-4a5f-8d72-570907a948fa", "")]
		[InlineData("1aeb8378-a102-4a5f-8d72-570907a948fa", " ")]
		[InlineData("1aeb8378-a102-4a5f-8d72-570907a948fa", "127.0.0.12.52.25")]
		public async Task CreateViewForAggregation_InvalidParametersFail(string? requestVideoId, string? requestIp)
		{
			Guid videoId = new Guid("1aeb8378-a102-4a5f-8d72-570907a948fa");
			AppUser user = new AppUser
			{
				Email = "email@email.com",
				IsEmailConfirmed = true,
				PublicUsername = "username",
				Username = "username"
			};
			Video video = new Video { Id = videoId, ThumbnailUrls = "", Title = "Vid", Urls = "" };
			VideoViewsAggregation views = new VideoViewsAggregation { Views = 99, Video = video };
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			db.AddRange(user, video, views);
			db.SaveChanges();
			var aggregator = new DefaultVideoViewsAggregatorService(Substitute.For<ILogger<DefaultVideoViewsAggregatorService>>(), db);
			var result = await aggregator.CreateViewForAggregation(requestVideoId, requestIp);
			Assert.True(result.IsError);
			Assert.True(400 == result.Code || 404 == result.Code);
			var viewFromDb = db.VideoViews.FirstOrDefault(_ => _.VideoId == video.Id && _.Ip == requestIp);
			Assert.Null(viewFromDb);
		}
		[Fact]
		public async Task Aggregate_Success()
		{
			Guid videoId = new Guid("1aeb8378-a102-4a5f-8d72-570907a948fa");
			AppUser user = new AppUser
			{
				Email = "email@email.com",
				IsEmailConfirmed = true,
				PublicUsername = "username",
				Username = "username"
			};
			Video video = new Video { ThumbnailUrls = "", Title = "Vid", Urls = "" };
			VideoSearchIndexing indexing = new VideoSearchIndexing { Video = video, ReindexingRequired = false, SearchIndexId = "nvm" };
			VideoViewsAggregation views = new VideoViewsAggregation { Views = 99, Video = video };
			VideoView view1 = new VideoView { Ip = IPAddress.Loopback.ToString(), Video = video };
			VideoView view2 = new VideoView { Ip = "168.0.0.1", Video = video };
			VideoView view3 = new VideoView { Ip = "169.0.0.2", Video = video };
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			db.AddRange(user, video, views, view1, view2, view3, indexing);
			db.SaveChanges();
			var aggregator = new DefaultVideoViewsAggregatorService(Substitute.For<ILogger<DefaultVideoViewsAggregatorService>>(), db);
			await aggregator.Aggregate();
			var updatedViews = db.VideoAggregatedViews.First(_ => _.Id == views.Id);
			Assert.Equal(102, updatedViews.Views);
			Assert.Empty(db.VideoViews.ToArray());
			var updatedIndexing = db.VideoSearchIndexing.First(_ => _.Id == indexing.Id);
			Assert.True(updatedIndexing.ReindexingRequired);
		}
	}
}
