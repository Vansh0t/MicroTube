using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;
using MicroTube.Data.Models;
using MicroTube.Services.ConfigOptions;

namespace MicroTube.Services.Search
{
	public class ElasticsearchVideoSearchRequestBuilder : IVideoSearchRequestBuilder<SearchRequest<VideoSearchIndex>>
	{
		private readonly IConfiguration _config;
		private readonly ILogger<ElasticsearchVideoSearchRequestBuilder> _logger;
		private readonly ISearchMetaProvider<SearchResponse<VideoSearchIndex>, ElasticsearchMeta> _searchMetaProvider;

		public ElasticsearchVideoSearchRequestBuilder(
			IConfiguration config,
			ILogger<ElasticsearchVideoSearchRequestBuilder> logger,
			ISearchMetaProvider<SearchResponse<VideoSearchIndex>, ElasticsearchMeta> searchMetaProvider)
		{
			_config = config;
			_logger = logger;
			_searchMetaProvider = searchMetaProvider;
		}
		public SearchRequest<VideoSearchIndex> Build(VideoSearchParameters parameters, string? meta)
		{
			VideoSearchOptions options = _config.GetRequiredByKey<VideoSearchOptions>(VideoSearchOptions.KEY);
			ElasticsearchMeta? parsedMeta = DeserializeMeta(meta);
			Query? textSearchQuery = BuildTextSearchQuery(parameters);
			ICollection<SortOptions>? sort = BuildVideoSearchSort(parameters);
			var searchRequest = new SearchRequest<VideoSearchIndex>(options.VideosIndexName);
			searchRequest.Size = Math.Min(parameters.BatchSize, options.PaginationMaxBatchSize);
			if(textSearchQuery != null)
				searchRequest.Query = textSearchQuery;
			if(sort != null)
				searchRequest.Sort = sort;
			if (parsedMeta != null && parsedMeta.LastSort != null)
				searchRequest.SearchAfter = parsedMeta.LastSort.ToArray();
			return searchRequest;
			
		}
		private ICollection<SortOptions>? BuildVideoSearchSort(VideoSearchParameters parameters)
		{
			List<SortOptions> sortOptions = new();
			var sortType = parameters.SortType;
			if (sortType == VideoSortType.Relevance && parameters.Text == null)
			{
				sortType = VideoSortType.Time; //TO DO: relevance is not available for textless search until some suggestion algorithm is implemented
			}
			if (sortType == VideoSortType.Relevance)
				return null;
			if (sortType == VideoSortType.Time)
			{
				sortOptions.Add(SortOptions.Field(nameof(VideoSearchIndex.UploadedAt)!, new FieldSort { Order = SortOrder.Desc, UnmappedType = FieldType.Date }));
			}
			if (sortType == VideoSortType.Views)
			{
				sortOptions.Add(SortOptions.Field(nameof(VideoSearchIndex.Views)!, new FieldSort { Order = SortOrder.Desc, UnmappedType = FieldType.Integer }));
			}
			//TO DO: needs better rating system
			if (sortType == VideoSortType.Rating)
			{
				sortOptions.Add(SortOptions.Field(nameof(VideoSearchIndex.Likes)!, new FieldSort { Order = SortOrder.Desc, UnmappedType = FieldType.Integer }));
			}
			return sortOptions;
		}
		private ICollection<Query>? BuildFilters(VideoTimeFilterType timeFilter, VideoLengthFilterType lengthFilter)
		{
			if (timeFilter == VideoTimeFilterType.None && lengthFilter == VideoLengthFilterType.None)
			{
				return null;
			}

			List<Query> filters = new List<Query>();
			Query? timeFilterQuery = BuildTimeFilterQuery(timeFilter);
			Query? lengthFilterQuery = BuildLengthFilterQuery(lengthFilter);
			if (timeFilterQuery != null)
				filters.Add(timeFilterQuery);
			if (lengthFilterQuery != null)
				filters.Add(lengthFilterQuery);
			return filters;
		}
		private Query? BuildTimeFilterQuery(VideoTimeFilterType timeFilter)
		{
			Query? timeQuery = null;
			DateTime now = DateTime.UtcNow;
			switch (timeFilter)
			{
				case VideoTimeFilterType.LastDay:
					timeQuery = new DateRangeQuery(new Field("uploadedAt")) { From = now.AddDays(-1), To = now};
					break;
				case VideoTimeFilterType.LastWeek:
					timeQuery = new DateRangeQuery(new Field("uploadedAt")) { From = now.AddDays(-7), To = now };
					break;
				case VideoTimeFilterType.LastMonth:
					timeQuery = new DateRangeQuery(new Field("uploadedAt")) { From = now.AddDays(-28), To = now };
					break;
				case VideoTimeFilterType.LastSixMonths:
					timeQuery = new DateRangeQuery(new Field("uploadedAt")) { From = now.AddDays(-28 * 6), To = now };
					break;
				case VideoTimeFilterType.LastYear:
					timeQuery = new DateRangeQuery(new Field("uploadedAt")) { From = now.AddDays(-28 * 12), To = now };
					break;
			}
			return timeQuery;
		}
		private Query? BuildLengthFilterQuery(VideoLengthFilterType lengthFilter)
		{
			var options = _config.GetRequiredByKey<VideoSearchOptions>(VideoSearchOptions.KEY);
			Query? lengthQuery = null;
			switch (lengthFilter)
			{
				case VideoLengthFilterType.Short:
					lengthQuery = new NumberRangeQuery(new Field("lengthSeconds")) { From = 0, To = options.ShortVideoSeconds };
					break;
				case VideoLengthFilterType.Medium:
					lengthQuery = new NumberRangeQuery(new Field("lengthSeconds"))
					{ From = options.ShortVideoSeconds, To = options.MediumVideoSeconds };
					break;
				case VideoLengthFilterType.Long:
					lengthQuery = new NumberRangeQuery(new Field("lengthSeconds"))
					{ From = options.MediumVideoSeconds };
					break;
			}
			return lengthQuery;
		}
		private Query? BuildTextSearchQuery(VideoSearchParameters parameters)
		{
			if (string.IsNullOrWhiteSpace(parameters.Text))
				return null;
			var matchQueryTitle = new MatchQuery(new Field("title")) { Query = parameters.Text, Boost = 2 };
			var matchQueryDescription = new MatchQuery(new Field("description")) { Query = parameters.Text };
			var filters = BuildFilters(parameters.TimeFilter, parameters.LengthFilter);
			var shouldQuery = new BoolQuery
			{
				MinimumShouldMatch = 1,
				Should = new Query[2] { matchQueryTitle, matchQueryDescription },
				Filter = filters
			};
			return shouldQuery;
		}
		private ElasticsearchMeta? DeserializeMeta(string? meta)
		{
			try
			{
				return _searchMetaProvider.DeserializeMeta(meta);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to deserialize meta due to exception");
				return null;
			}
		}
	}
}
