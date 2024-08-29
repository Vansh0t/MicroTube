using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicroTube.Data.Models;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.Search;
using NSubstitute;

namespace MicroTube.Tests.Unit.Search
{
	public class ElasticsearchVideoSearchRequestBuilderTests
	{
		[Theory]
		[InlineData("some text", VideoTimeFilterType.LastDay, VideoLengthFilterType.Medium, VideoSortType.Time, null)]
		[InlineData("some tex", VideoTimeFilterType.LastWeek, VideoLengthFilterType.Medium, VideoSortType.Time, "meta")]
		[InlineData("some tex", VideoTimeFilterType.LastMonth, VideoLengthFilterType.Medium, VideoSortType.Time, null)]
		[InlineData("some tex", VideoTimeFilterType.LastSixMonths, VideoLengthFilterType.Medium, VideoSortType.Time, "meta")]
		[InlineData("", VideoTimeFilterType.LastWeek, VideoLengthFilterType.Medium, VideoSortType.Time, "meta")]
		[InlineData(" ", VideoTimeFilterType.LastMonth, VideoLengthFilterType.Medium, VideoSortType.Time, null)]
		[InlineData(null, VideoTimeFilterType.LastSixMonths, VideoLengthFilterType.Medium, VideoSortType.Time, "meta")]
		[InlineData("some text", VideoTimeFilterType.LastYear, VideoLengthFilterType.Medium, VideoSortType.Time, null)]
		[InlineData("some text", VideoTimeFilterType.LastYear, VideoLengthFilterType.Medium, VideoSortType.Time, "meta")]
		[InlineData("some text", VideoTimeFilterType.LastYear, VideoLengthFilterType.None, VideoSortType.Time, null)]
		[InlineData("some text", VideoTimeFilterType.LastYear, VideoLengthFilterType.Short, VideoSortType.Time, "meta")]
		[InlineData("some text", VideoTimeFilterType.LastYear, VideoLengthFilterType.Long, VideoSortType.Time, null)]
		[InlineData("some text", VideoTimeFilterType.LastYear, VideoLengthFilterType.Long, VideoSortType.Relevance, "meta")]
		[InlineData("some text", VideoTimeFilterType.LastYear, VideoLengthFilterType.Long, VideoSortType.Views, null)]
		[InlineData("some text", VideoTimeFilterType.LastYear, VideoLengthFilterType.None, VideoSortType.Rating, "meta")]
		public void Build_Success(string? searchText, VideoTimeFilterType timeFilter, VideoLengthFilterType lengthFilter, VideoSortType sortType, string? meta)
		{
			var config = new ConfigurationBuilder()
				.AddConfigObject(VideoSearchOptions.KEY, new VideoSearchOptions(6, "videos", "video_suggestions", 30, 40, 60, 40))
				.Build();
			var metaProvider = Substitute.For<ISearchMetaProvider<SearchResponse<VideoSearchIndex>, ElasticsearchMeta>>();
			var deserializedMeta = new ElasticsearchMeta { LastSort = new List<FieldValue> { 1250, 2515 } };
			metaProvider.DeserializeMeta("meta").Returns(deserializedMeta);
			metaProvider.DeserializeMeta(Arg.Is<string?>(_=>_==null)).Returns(_=> null);
			var requestBuilder = new ElasticsearchVideoSearchRequestBuilder(
				config,
				Substitute.For<ILogger<ElasticsearchVideoSearchRequestBuilder>>(),
				metaProvider);
			var videoSearchParameters = new VideoSearchParameters
			{
				BatchSize = 20,
				LengthFilter = lengthFilter,
				TimeFilter = timeFilter,
				SortType = sortType,
				Text = searchText
			};
			var request = requestBuilder.Build(videoSearchParameters, meta);
			if (string.IsNullOrWhiteSpace(searchText))
				Assert.Null(request.Query); // filters are ignored for textless search as of now
			else
			{
				Assert.NotNull(request.Query);
				bool hasBoolQuery = request.Query.TryGet<BoolQuery>(out var boolQuery);
				Assert.True(hasBoolQuery);
				Assert.NotNull(boolQuery);
				Assert.NotNull(boolQuery.Filter);
				Assert.True(boolQuery.Filter.Count > 0);
				Assert.NotNull(boolQuery.Should);
				Assert.True(boolQuery.Should.Count > 0);
				Assert.NotNull(boolQuery.MinimumShouldMatch);
			}
				
			if (sortType == VideoSortType.Relevance && searchText != null)
				Assert.Null(request.Sort);
			else
			{
				Assert.NotNull(request.Sort);
				Assert.Single(request.Sort);
			}
			if (meta != null)
				Assert.Equal(request.SearchAfter, deserializedMeta.LastSort);
			else
				Assert.Null(request.SearchAfter);
		}
	}
}
