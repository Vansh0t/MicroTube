using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Transport.Products.Elasticsearch;
using Microsoft.Extensions.Logging;
using MicroTube.Data.Models;
using MicroTube.Services.Search;
using MicroTube.Services.Validation;
using NSubstitute;

namespace MicroTube.Tests.Unit.Search
{
	public class ElasticsearchVideoSearchResultBuilderTests
	{
		[Theory]
		[InlineData("meta")]
		[InlineData(null)]
		public void Build_Success(string? meta)
		{
			var deserializedMeta = new ElasticsearchMeta { LastSort = new List<FieldValue> { 1250, 2515 } };
			var resultContent = new List<VideoSearchIndex>()
			{
				new VideoSearchIndex("id1", "title1", "descr1", "title1", 2, 5, 6, 60, DateTime.UtcNow),
				new VideoSearchIndex("id2", "title2", "descr2", "title2", 6, 2, 5, 30, DateTime.UtcNow)
			};
			var searchResponse = new SearchResponse<VideoSearchIndex>()
			{
				HitsMetadata = new HitsMetadata<VideoSearchIndex>()
				{
					Hits = new List<Hit<VideoSearchIndex>>
					{
						new Hit<VideoSearchIndex> { Source = resultContent [0]},
						new Hit<VideoSearchIndex> { Source = resultContent [1]}
					}
				}
			};
			var metaProvider = Substitute.For<ISearchMetaProvider<SearchResponse<VideoSearchIndex>, ElasticsearchMeta>>();
			metaProvider.BuildMeta(searchResponse).Returns(deserializedMeta);
			metaProvider.SerializeMeta(deserializedMeta).Returns(_ => meta);
			var responseValidator = Substitute.For<ISearchResponseValidator<ElasticsearchResponse>>();
			responseValidator.Validate(Arg.Any<ElasticsearchResponse>()).Returns(true);
			
			
			var builder = new ElasticsearchVideoSearchResultBuilder(
				Substitute.For<ILogger<ElasticsearchVideoSearchResultBuilder>>(),
				metaProvider, responseValidator);
			VideoSearchResult searchResult = builder.Build(searchResponse, meta);
			Assert.True(resultContent[0].IsEqualByContentValues(searchResult.Indices.First()));
			Assert.True(resultContent[1].IsEqualByContentValues(searchResult.Indices.Last()));
			Assert.Equal(meta, searchResult.Meta);
		}
		[Theory]
		[InlineData("meta")]
		[InlineData(null)]
		public void Build_EndOfDataSuccess(string? meta)
		{
			var deserializedMeta = new ElasticsearchMeta { LastSort = new List<FieldValue> { 1250, 2515 } };
			var searchResponse = new SearchResponse<VideoSearchIndex>()
			{
				HitsMetadata = new HitsMetadata<VideoSearchIndex>()
				{
					Hits = new List<Hit<VideoSearchIndex>>()
				}
			};
			var metaProvider = Substitute.For<ISearchMetaProvider<SearchResponse<VideoSearchIndex>, ElasticsearchMeta>>();
			var responseValidator = Substitute.For<ISearchResponseValidator<ElasticsearchResponse>>();
			responseValidator.Validate(Arg.Any<ElasticsearchResponse>()).Returns(true);


			var builder = new ElasticsearchVideoSearchResultBuilder(
				Substitute.For<ILogger<ElasticsearchVideoSearchResultBuilder>>(),
				metaProvider, responseValidator);
			VideoSearchResult searchResult = builder.Build(searchResponse, meta);
			Assert.Empty(searchResult.Indices);
			Assert.Equal(meta, searchResult.Meta);
		}
		[Fact]
		public void Build_InvalidResponseFail()
		{
			var deserializedMeta = new ElasticsearchMeta { LastSort = new List<FieldValue> { 1250, 2515 } };
			var searchResponse = new SearchResponse<VideoSearchIndex>()
			{
				HitsMetadata = new HitsMetadata<VideoSearchIndex>()
				{
					Hits = new List<Hit<VideoSearchIndex>>()
				}
			};
			var metaProvider = Substitute.For<ISearchMetaProvider<SearchResponse<VideoSearchIndex>, ElasticsearchMeta>>();
			var responseValidator = Substitute.For<ISearchResponseValidator<ElasticsearchResponse>>();
			responseValidator.Validate(searchResponse).Returns(false);
			var builder = new ElasticsearchVideoSearchResultBuilder(
				Substitute.For<ILogger<ElasticsearchVideoSearchResultBuilder>>(),
				metaProvider, responseValidator);
			Assert.ThrowsAny<ExternalServiceException>(()=>builder.Build(searchResponse, null));
		}
	}
}
