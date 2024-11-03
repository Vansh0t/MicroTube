using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Transport;
using Elastic.Transport.Products.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.Search.Videos;
using MicroTube.Services.Validation;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Collections.Immutable;

namespace MicroTube.Tests.Unit.Search
{
	public class ElasticVideoSearchServiceTests
	{
		[Fact]
		public async Task IndexVideo_Success()
		{
			string elasticApiKey = "elastic_key";
			string elasticApiUrl = "elastic_url";
			DateTime lastIndexingTime = DateTime.UtcNow - TimeSpan.FromSeconds(5);
			Video videoToIndex = new Video
			{
				Id = Guid.NewGuid(),
				Title = "title",
				Description = "description",
				Urls = "",
				ThumbnailUrls = "",
				VideoReactions = new VideoReactionsAggregation { Dislikes = 1, Likes = 2 },
				VideoViews = new VideoViewsAggregation { Views = 3 },
				VideoIndexing = new VideoSearchIndexing { Id = Guid.NewGuid(), ReindexingRequired = true, LastIndexingTime = lastIndexingTime },
			};
			VideoSearchIndex expectedIndex = new(
				videoToIndex.Id.ToString(),
				videoToIndex.Title,
				videoToIndex.Description,
				videoToIndex.Title,
				videoToIndex.VideoViews.Views, 
				videoToIndex.VideoReactions.Likes,
				videoToIndex.VideoReactions.Dislikes,
				videoToIndex.LengthSeconds,
				DateTime.UtcNow, null);
			IVideoSearchService searchService = CreateVideoSearchService(
				elasticApiKey,
				elasticApiUrl,
				new VideoSearchParameters(),
				new VideoSearchResult(new VideoSearchIndex[0]));
			var result = await searchService.IndexVideo(videoToIndex);
			Assert.False(result.IsError);
			var updatedVideo = result.GetRequiredObject();
			Assert.False(updatedVideo.VideoIndexing!.ReindexingRequired);
			Assert.NotNull(updatedVideo.VideoIndexing.SearchIndexId);
			Assert.NotEqual(lastIndexingTime, updatedVideo.VideoIndexing.LastIndexingTime);
		}
		[Fact]
		public async Task GetVideos_Success()
		{
			string elasticApiKey = "elastic_key";
			string elasticApiUrl = "elastic_url";
			var videoSearchParameters = new VideoSearchParameters();
			var videoSearchResult = new VideoSearchResult(new VideoSearchIndex[0]);
			IVideoSearchService searchService = CreateVideoSearchService(
				elasticApiKey,
				elasticApiUrl,
				videoSearchParameters,
				videoSearchResult);
			var result = await searchService.GetVideos(videoSearchParameters, null);
			Assert.False(result.IsError);
			Assert.Same(videoSearchResult, result.GetRequiredObject());
		}
		[Fact]
		public async Task GetVideos_InvalidParametersFail()
		{
			IVideoSearchDataAccess videoSearchAccess = Substitute.For<IVideoSearchDataAccess>();
			videoSearchAccess.IndexVideo(Arg.Any<VideoSearchIndex>()).Returns(_ => _.Arg<VideoSearchIndex>().Id);
			var config = new ConfigurationBuilder().Build();
			ElasticsearchClient client = Substitute.For<ElasticsearchClient>();
			var requestBuilder = Substitute.For<IVideoSearchRequestBuilder<SearchRequest<VideoSearchIndex>>>();
			requestBuilder.Build(Arg.Any<VideoSearchParameters>(), Arg.Any<string?>()).Throws(new Exception("Invalid parameters"));
			var resultBuilder = Substitute.For<IVideoSearchResultBuilder<SearchResponse<VideoSearchIndex>>>();
			var searchService = new ElasticVideoSearchService(
				config,
				client,
				Substitute.For<ILogger<ElasticVideoSearchService>>(),
				videoSearchAccess,
				requestBuilder, resultBuilder, Substitute.For<ISearchResponseValidator<ElasticsearchResponse>>());
			var result = await searchService.GetVideos(new VideoSearchParameters(), null);
			Assert.True(result.IsError);
			Assert.Equal(500, result.Code);
		}
		[Fact]
		public async Task GetVideos_InvalidResponseFail()
		{
			IVideoSearchDataAccess videoSearchAccess = Substitute.For<IVideoSearchDataAccess>();
			videoSearchAccess.IndexVideo(Arg.Any<VideoSearchIndex>()).Returns(_ => _.Arg<VideoSearchIndex>().Id);
			var config = new ConfigurationBuilder().Build();
			ElasticsearchClient client = Substitute.For<ElasticsearchClient>();
			var requestBuilder = Substitute.For<IVideoSearchRequestBuilder<SearchRequest<VideoSearchIndex>>>();
			var request = new SearchRequest<VideoSearchIndex>();
			var invalidResponse = new SearchResponse<VideoSearchIndex>();
			requestBuilder.Build(Arg.Any<VideoSearchParameters>(), Arg.Any<string?>()).Returns(request);
			client.SearchAsync<VideoSearchIndex>(request).Returns(invalidResponse);
			var resultBuilder = Substitute.For<IVideoSearchResultBuilder<SearchResponse<VideoSearchIndex>>>();
			resultBuilder.Build(invalidResponse, Arg.Any<string?>()).Throws(new Exception("Invalid response"));
			var searchService = new ElasticVideoSearchService(
				config,
				client,
				Substitute.For<ILogger<ElasticVideoSearchService>>(),
				videoSearchAccess,
				requestBuilder, resultBuilder, Substitute.For<ISearchResponseValidator<ElasticsearchResponse>>());
			var result = await searchService.GetVideos(new VideoSearchParameters(), null);
			Assert.True(result.IsError);
			Assert.Equal(500, result.Code);
		}
		[Fact]
		public async Task GetSuggestions_Success()
		{
			IEnumerable<VideoSearchIndex> suggestions = new List<VideoSearchIndex> 
			{ 
				new VideoSearchIndex("id1", "title1", "desc", "title1", 1, 1, 1, 10, DateTime.UtcNow, null),
				new VideoSearchIndex("id2", "title2", "desc", "title2", 1, 1, 1, 10, DateTime.UtcNow, null),
			};
			IVideoSearchDataAccess videoSearchAccess = Substitute.For<IVideoSearchDataAccess>();
			videoSearchAccess.IndexVideo(Arg.Any<VideoSearchIndex>()).Returns(_ => _.Arg<VideoSearchIndex>().Id);
			var config = new ConfigurationBuilder()
				.AddConfigObject(VideoSearchOptions.KEY, new VideoSearchOptions(6, "videos", "video_search_suggestions", 30, 40, 60, 40))
				.Build();
			ElasticsearchClient client = Substitute.For<ElasticsearchClient>();
			SearchResponse<VideoSearchIndex> mockElasticResponse = new()
			{
				HitsMetadata = new HitsMetadata<VideoSearchIndex> { Hits = suggestions.Select(_ => new Hit<VideoSearchIndex>() { Source = _ }).ToImmutableArray() },
			};
			var validator = Substitute.For<ISearchResponseValidator<ElasticsearchResponse>>();
			validator.Validate(mockElasticResponse).Returns(true);
			validator.Validate(Arg.Is<ElasticsearchResponse>(_ => _ != mockElasticResponse)).Returns(false);
			client.SearchAsync<VideoSearchIndex>(Arg.Any<Action<SearchRequestDescriptor<VideoSearchIndex>>>()).ReturnsForAnyArgs(mockElasticResponse);
			var requestBuilder = Substitute.For<IVideoSearchRequestBuilder<SearchRequest<VideoSearchIndex>>>();
			var resultBuilder = Substitute.For<IVideoSearchResultBuilder<SearchResponse<VideoSearchIndex>>>();
			var searchService = new ElasticVideoSearchService(
				config,
				client,
				Substitute.For<ILogger<ElasticVideoSearchService>>(),
				videoSearchAccess,
				requestBuilder, resultBuilder, validator);
			var result = await searchService.GetSuggestions("text");
			Assert.False(result.IsError);
			Assert.Equal(2, result.GetRequiredObject().Count());
		}
		[Fact]
		public async Task GetSuggestions_ResponseFail()
		{
			IEnumerable<VideoSearchIndex> suggestions = new List<VideoSearchIndex>
			{
				new VideoSearchIndex("id1", "title1", "desc", "title1", 1, 1, 1, 10, DateTime.UtcNow, null),
				new VideoSearchIndex("id2", "title2", "desc", "title2", 1, 1, 1, 10, DateTime.UtcNow, null),
			};
			IVideoSearchDataAccess videoSearchAccess = Substitute.For<IVideoSearchDataAccess>();
			videoSearchAccess.IndexVideo(Arg.Any<VideoSearchIndex>()).Returns(_ => _.Arg<VideoSearchIndex>().Id);
			var config = new ConfigurationBuilder()
				.AddConfigObject(VideoSearchOptions.KEY, new VideoSearchOptions(6, "videos", "video_search_suggestions", 30, 40, 60, 40))
				.Build();
			ElasticsearchClient client = Substitute.For<ElasticsearchClient>();
			SearchResponse<VideoSearchIndex> mockElasticResponse = new()
			{
				HitsMetadata = new HitsMetadata<VideoSearchIndex> { Hits = suggestions.Select(_ => new Hit<VideoSearchIndex>() { Source = _ }).ToImmutableArray() },
			};
			var validator = Substitute.For<ISearchResponseValidator<ElasticsearchResponse>>();
			validator.Validate(mockElasticResponse).Returns(false);
			client.SearchAsync<VideoSearchIndex>(Arg.Any<Action<SearchRequestDescriptor<VideoSearchIndex>>>()).ReturnsForAnyArgs(mockElasticResponse);
			var requestBuilder = Substitute.For<IVideoSearchRequestBuilder<SearchRequest<VideoSearchIndex>>>();
			var resultBuilder = Substitute.For<IVideoSearchResultBuilder<SearchResponse<VideoSearchIndex>>>();
			var searchService = new ElasticVideoSearchService(
				config,
				client,
				Substitute.For<ILogger<ElasticVideoSearchService>>(),
				videoSearchAccess,
				requestBuilder, resultBuilder, validator);
			var result = await searchService.GetSuggestions("text");
			Assert.True(result.IsError);
			Assert.Equal(500, result.Code);
			Assert.Null(result.ResultObject);
		}
		private IVideoSearchService CreateVideoSearchService(
			string elasticApiKey,
			string elasticApiUrl,
			VideoSearchParameters validSearchParameters,
			VideoSearchResult validSearchResult)
		{
			IVideoSearchDataAccess videoSearchAccess = Substitute.For<IVideoSearchDataAccess>();
			videoSearchAccess.IndexVideo(Arg.Any<VideoSearchIndex>()).Returns(_ => _.Arg<VideoSearchIndex>().Id);
			var config = new ConfigurationBuilder()
				.AddConfigObject(VideoSearchOptions.KEY, new VideoSearchOptions(6, "videos", "video_search_suggestions", 30, 40, 60, 40))
				.AddConfigObject(ElasticSearchOptions.KEY, new ElasticSearchOptions(elasticApiKey, elasticApiUrl))
				.Build();
			SearchRequest<VideoSearchIndex> mockElasticRequest = new();
			SearchResponse<VideoSearchIndex> mockElasticResponse = new();
			ElasticsearchClient client = Substitute.For<ElasticsearchClient>();
			client.SearchAsync<VideoSearchIndex>(mockElasticRequest).Returns(mockElasticResponse);
			var requestBuilder = Substitute.For<IVideoSearchRequestBuilder<SearchRequest<VideoSearchIndex>>>();
			requestBuilder.Build(validSearchParameters, Arg.Any<string?>()).Returns(mockElasticRequest);
			requestBuilder.Build(Arg.Is<VideoSearchParameters>(_=>_ != validSearchParameters), Arg.Any<string?>()).Throws(new Exception("Invalid parameters"));
			var resultBuilder = Substitute.For<IVideoSearchResultBuilder<SearchResponse<VideoSearchIndex>>>();
			resultBuilder.Build(mockElasticResponse, Arg.Any<string?>()).Returns(validSearchResult);
			resultBuilder.Build(Arg.Is<SearchResponse<VideoSearchIndex>>(_ => _ != mockElasticResponse), Arg.Any<string?>()).Throws(new Exception("Invalid parameters"));
			return new ElasticVideoSearchService(
				config,
				client,
				Substitute.For<ILogger<ElasticVideoSearchService>>(),
				videoSearchAccess,
				requestBuilder, resultBuilder, Substitute.For<ISearchResponseValidator<ElasticsearchResponse>>());
		}
	}
}
