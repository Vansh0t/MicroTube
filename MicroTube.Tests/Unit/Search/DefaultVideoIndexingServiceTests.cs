using Microsoft.Extensions.Logging;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services;
using MicroTube.Services.Search;
using MicroTube.Tests.Utils;
using NSubstitute;

namespace MicroTube.Tests.Unit.Search
{
	public class DefaultVideoIndexingServiceTests
	{
		[Fact]
		public async Task EnsureVideoIndices_FullSuccess()
		{
			MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			string searchIndexId = "search_index_id";
			DateTime expectedReindexTime = DateTime.UtcNow;
			var video1 = new Video
			{
				ThumbnailUrls = "",
				Urls = "",
				Title = "Vid1",
			};
			video1.VideoIndexing = new VideoSearchIndexing { LastIndexingTime = DateTime.UtcNow, SearchIndexId = null, ReindexingRequired = true, Video = video1 };
			video1.VideoReactions = new VideoReactionsAggregation { Dislikes = 0, Likes = 0, Video = video1 };
			video1.VideoViews = new VideoViewsAggregation { Views = 0, Video = video1 };
			var video2 = new Video
			{
				ThumbnailUrls = "",
				Urls = "",
				Title = "Vid2",
			};
			video2.VideoIndexing = new VideoSearchIndexing { LastIndexingTime = DateTime.UtcNow, SearchIndexId = null, ReindexingRequired = true, Video = video2 };
			video2.VideoReactions = new VideoReactionsAggregation { Dislikes = 0, Likes = 0, Video = video2 };
			video2.VideoViews = new VideoViewsAggregation { Views = 0, Video = video2 };
			db.AddRange(video1, video2);
			db.SaveChanges();
			IVideoIndexingService videoIndexing = CreateVideoIndexingService(db, expectedReindexTime, searchIndexId);
			await videoIndexing.EnsureVideoIndices();
			var updatedVideo1 = db.VideoSearchIndexing.First(_ => _.VideoId == video1.Id);
			var updatedVideo2 = db.VideoSearchIndexing.First(_ => _.VideoId == video2.Id);
			Assert.Equal(searchIndexId, updatedVideo1.SearchIndexId);
			Assert.Equal(expectedReindexTime, updatedVideo1.LastIndexingTime);
			Assert.False(updatedVideo1.ReindexingRequired);
			Assert.Equal(searchIndexId, updatedVideo2.SearchIndexId);
			Assert.Equal(expectedReindexTime, updatedVideo2.LastIndexingTime);
			Assert.False(updatedVideo2.ReindexingRequired);
		}
		private IVideoIndexingService CreateVideoIndexingService(MicroTubeDbContext db, DateTime validReindexedTime, string validIndexId)
		{
			ILogger<DefaultVideoIndexingService> logger = Substitute.For<ILogger<DefaultVideoIndexingService>>();
			IVideoSearchService videoSearch = Substitute.For<IVideoSearchService>();
			videoSearch.IndexVideo(Arg.Any<Video>()).ReturnsForAnyArgs(_ => {
				Video video = _.Arg<Video>();
				video.VideoIndexing!.ReindexingRequired = false;
				video.VideoIndexing.SearchIndexId = validIndexId;
				video.VideoIndexing.LastIndexingTime = validReindexedTime;
				return ServiceResult<Video>.Success(video);
			});
			return new DefaultVideoIndexingService(logger, videoSearch, db);
		}
	}
}
