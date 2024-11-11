using AutoFixture;
using AutoFixture.Kernel;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicroTube.Data.Models;
using MicroTube.Data.Models.Comments;
using MicroTube.Data.Models.Videos;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.Search;
using MicroTube.Services.Search.Comments;
using MicroTube.Tests.Mock.Models;
using MicroTube.Tests.Utils;

namespace MicroTube.Tests.Unit.Search
{
    public class VideoCommentSearchServiceTests
	{
		private readonly Fixture _commentsFixture;
		private readonly AppUser _videoUploader = new AppUser
		{
			Id = Guid.NewGuid(),
			Email = "user@email.com",
			IsEmailConfirmed = true,
			PublicUsername = "user",
			Username = "user",
			Authentication = new MockAuthenticationData()
		};
		private readonly Video _video = new Video { Id = Guid.NewGuid(), ThumbnailUrls = "", Title = "Video", Urls = "" };
		public VideoCommentSearchServiceTests()
		{
			_commentsFixture = new Fixture();
			
			_commentsFixture.Customizations.Add(new TypeRelay(typeof(AuthenticationData), typeof(MockAuthenticationData)));
			_commentsFixture.Behaviors.Add(new OmitOnRecursionBehavior());
		}
		[Theory]
		[InlineData(20, 100)]
		[InlineData(30, 100)]
		[InlineData(5, 100)]
		[InlineData(30, 1000)]
		[InlineData(30, 10000)]
		[InlineData(20, 10)]
		[InlineData(20, 0)]
		public async Task GetComments_Top_Success(int batchSize, int commentsAmount)
		{
			var random = new Random();
			var comments = _commentsFixture
				.Build<VideoComment>()
				.With(_ => _.User, _videoUploader)
				.With(_ => _.UserId, _videoUploader.Id)
				.With(_ => _.Video, _video)
				.With(_ => _.VideoId, _video.Id)
				.CreateMany(commentsAmount).ToArray();
			foreach(var comment in comments)
			{
				int likes = random.Next(0, 10000);
				int dislikes = random.Next(0, 10000);
				int difference = likes - dislikes;
				var reactions = new VideoCommentReactionsAggregation {Likes = likes, Dislikes = dislikes, Difference = difference, Comment = comment, CommentId = comment.Id };
				comment.CommentReactionsAggregation = reactions;
				comment.Time = DateTime.UtcNow + TimeSpan.FromMilliseconds(random.Next(2, 100000000));
			}
			var db = Database.CreateSqliteInMemoryMock();
			db.AddRange(comments);
			db.SaveChanges();
			var commentsFromDb = await db.VideoComments.Include(_=>_.CommentReactionsAggregation).ToArrayAsync();
			ISearchMetaProvider<IEnumerable<VideoComment>, VideoCommentSearchMeta> metaProvider = new VideoCommentSearchMetaProvider();
			var config = new ConfigurationBuilder().AddConfigObject<VideoCommentSearchOptions>(VideoCommentSearchOptions.KEY, new VideoCommentSearchOptions(batchSize)).Build();
			var commentsSearch = new VideoCommentSearchService(db, metaProvider, config, Substitute.For<ILogger<VideoCommentSearchService>>());
			var searchParameters = new VideoCommentSearchParameters
			{
				BatchSize = batchSize,
				SortType = VideoCommentSortType.Top
			};
			int batchesCount = 0;
			int desiredBatchesCount = (int)Math.Ceiling((double)commentsAmount/batchSize);
			List<VideoComment> retrievedComments = new List<VideoComment>(commentsAmount);
			string? meta = null;
			while(true)
			{
				
				var result = await commentsSearch.GetComments(_video.Id.ToString(), searchParameters, meta);
				Assert.False(result.IsError);
				var searchResult = result.GetRequiredObject();
				meta = searchResult.Meta;
				if(searchResult.Comments.Count() == 0)
				{
					break;
				}
				batchesCount++;
				retrievedComments.AddRange(searchResult.Comments);
				if (batchesCount > desiredBatchesCount)
				{
					throw new Exception($"Batches count overflow: Desired: {desiredBatchesCount}");
				}
			}
			var retrievedIdsHashSet = retrievedComments.Select(_=>_.Id).ToHashSet();
			var fromDbIdsHashSet = commentsFromDb.Select(_=>_.Id).ToHashSet();
			var notRetrieved = fromDbIdsHashSet.Where(_ => !retrievedIdsHashSet.Contains(_)).Select(_ => commentsFromDb.First(__=>__.Id == _));
			Assert.Empty(notRetrieved);
			Assert.Equal(commentsAmount, retrievedComments.Count);
			Assert.Equal(desiredBatchesCount, batchesCount);
			var desiredIds = commentsFromDb.OrderByDescending(_ => _.CommentReactionsAggregation!.Difference).ThenByDescending(_ => _.Time).Select(_ => _.Id);
			var retrievedIds = retrievedComments.Select(_ => _.Id);
			Assert.Equal(desiredIds, retrievedIds);
		}
		[Theory]
		[InlineData(20, 100)]
		[InlineData(30, 100)]
		[InlineData(5, 100)]
		[InlineData(30, 1000)]
		[InlineData(30, 10000)]
		[InlineData(20, 10)]
		[InlineData(20, 0)]
		public async Task GetComments_Newest_Success(int batchSize, int commentsAmount)
		{
			var random = new Random();
			var comments = _commentsFixture
				.Build<VideoComment>()
				.With(_ => _.User, _videoUploader)
				.With(_ => _.UserId, _videoUploader.Id)
				.With(_ => _.Video, _video)
				.With(_ => _.VideoId, _video.Id)
				.CreateMany(commentsAmount).ToArray();
			foreach (var comment in comments)
			{
				int likes = random.Next(0, 10000);
				int dislikes = random.Next(0, 10000);
				int difference = likes - dislikes;
				var reactions = new VideoCommentReactionsAggregation { Likes = likes, Dislikes = dislikes, Difference = difference, Comment = comment, CommentId = comment.Id };
				comment.CommentReactionsAggregation = reactions;
				comment.Time = DateTime.UtcNow + TimeSpan.FromMilliseconds(random.Next(2, 100000000));
			}
			var db = Database.CreateSqliteInMemoryMock();
			db.AddRange(comments);
			db.SaveChanges();
			var commentsFromDb = await db.VideoComments.Include(_ => _.CommentReactionsAggregation).ToArrayAsync();
			ISearchMetaProvider<IEnumerable<VideoComment>, VideoCommentSearchMeta> metaProvider = new VideoCommentSearchMetaProvider();
			var config = new ConfigurationBuilder().AddConfigObject<VideoCommentSearchOptions>(VideoCommentSearchOptions.KEY, new VideoCommentSearchOptions(batchSize)).Build();
			var commentsSearch = new VideoCommentSearchService(db, metaProvider, config, Substitute.For<ILogger<VideoCommentSearchService>>());
			var searchParameters = new VideoCommentSearchParameters
			{
				BatchSize = batchSize,
				SortType = VideoCommentSortType.Newest
			};
			int batchesCount = 0;
			int desiredBatchesCount = (int)Math.Ceiling((double)commentsAmount / batchSize);
			List<VideoComment> retrievedComments = new List<VideoComment>(commentsAmount);
			string? meta = null;
			while (true)
			{

				var result = await commentsSearch.GetComments(_video.Id.ToString(), searchParameters, meta);
				Assert.False(result.IsError);
				var searchResult = result.GetRequiredObject();
				meta = searchResult.Meta;
				if (searchResult.Comments.Count() == 0)
				{
					break;
				}
				batchesCount++;
				retrievedComments.AddRange(searchResult.Comments);
				if (batchesCount > desiredBatchesCount)
				{
					throw new Exception($"Batches count overflow: Desired: {desiredBatchesCount}");
				}
			}
			var retrievedIdsHashSet = retrievedComments.Select(_ => _.Id).ToHashSet();
			var fromDbIdsHashSet = commentsFromDb.Select(_ => _.Id).ToHashSet();
			var notRetrieved = fromDbIdsHashSet.Where(_ => !retrievedIdsHashSet.Contains(_)).Select(_ => commentsFromDb.First(__ => __.Id == _));
			Assert.Empty(notRetrieved);
			Assert.Equal(commentsAmount, retrievedComments.Count);
			Assert.Equal(desiredBatchesCount, batchesCount);
			var desiredIds = commentsFromDb.OrderByDescending(_ => _.Time).Select(_ => _.Id);
			var retrievedIds = retrievedComments.Select(_ => _.Id);
			Assert.Equal(desiredIds, retrievedIds);
		}
	}
}
