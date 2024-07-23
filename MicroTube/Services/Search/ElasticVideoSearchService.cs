using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Options;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.Cryptography;
using System.Collections.Immutable;

namespace MicroTube.Services.Search
{
    public class ElasticVideoSearchService : IVideoSearchService
	{

		private readonly IVideoSearchDataAccess _searchDataAccess;
		private readonly IConfiguration _config;
		private readonly ElasticsearchClient _client;
		private readonly ILogger<ElasticVideoSearchService> _logger;
		private readonly IMD5HashProvider _md5Hash;

		public ElasticVideoSearchService(
			IConfiguration config,
			ElasticsearchClient client,
			ILogger<ElasticVideoSearchService> logger,
			IVideoSearchDataAccess searchDataAccess,
			IMD5HashProvider md5Hash)
		{
			_config = config;
			_client = client;
			_logger = logger;
			_searchDataAccess = searchDataAccess;
			_md5Hash = md5Hash;
		}

		public async Task<IServiceResult<Video>> IndexVideo(Video video)
		{
			var videoIndexData = new VideoSearchIndex(video.Id.ToString(),
				video.Title,
				video.Description,
				video.Title,
				video.Views,
				video.Likes,
				video.LengthSeconds,
				video.UploadTime);
			try
			{
				string indexId = await _searchDataAccess.IndexVideo(videoIndexData);
				video.SearchIndexId = indexId;
				_logger.LogInformation("ElasticSearch indexing done: " + indexId);	
			}
			catch (Exception e)
			{
				var error = $"Failed to index video {video.Id}";
				_logger.LogError(e, $"Failed to index search suggestion {video.Id}");
				return ServiceResult<Video>.Fail(500, error);
			}
			
			return ServiceResult<Video>.Success(video);
		}
		public async Task<IServiceResult<IReadOnlyCollection<VideoSearchIndex>>> GetVideos(
			string text,
			VideoSortType sortType = VideoSortType.Relevance,
			VideoTimeFilterType timeFilter = VideoTimeFilterType.None,
			VideoLengthFilterType lengthFilter = VideoLengthFilterType.None)
		{
			VideoSearchOptions options = _config.GetRequiredByKey<VideoSearchOptions>(VideoSearchOptions.KEY);
			var matchQueryTitle = new MatchQuery(new Field("title")) { Query = text };
			var matchQueryDescription = new MatchQuery(new Field("description")) { Query = text };
			var filters = BuildFilters(timeFilter, lengthFilter);
			var shouldQuery = new BoolQuery
			{
				Should = new Query[2] { matchQueryTitle, matchQueryDescription },
				Filter = filters
			};
			var sort = BuildVideoSearchSort(sortType);
			var result = await _client.SearchAsync<VideoSearchIndex>(search =>
			{
				search.Index(options.VideosIndexName)
				.From(0)
				.Query(shouldQuery);
				if (sort != null)
				{
					search.Sort(sort);
				}
			});
			if (!result.IsValidResponse)
			{
				_logger.LogError("Failed to get suggestions attempt from ElasticSearch. " + result.ToString());
				return ServiceResult<IReadOnlyCollection<VideoSearchIndex>>.FailInternal();
			}
			return ServiceResult<IReadOnlyCollection<VideoSearchIndex>>.Success(result.Documents);
		}
		public async Task<IServiceResult<IReadOnlyCollection<string>>> GetSuggestions(string text)
		{
			VideoSearchOptions options = _config.GetRequiredByKey<VideoSearchOptions>(VideoSearchOptions.KEY);
			MultiMatchQuery query = new MultiMatchQuery()
			{
				Query = text,
				Type = TextQueryType.PhrasePrefix,
				Analyzer = "simple",
				Fields = Fields.FromStrings(new string[3]
				{
					"titleSuggestion",
					"titleSuggestion._2gram",
					"titleSuggestion._3gram"
				})
			};
			var response = await _client.SearchAsync<VideoSearchIndex>(search =>
			{
				search.Index(options.VideosIndexName)
				.Query(query);
			});

			if (!response.IsValidResponse)
			{
				_logger.LogError("Failed to get suggestions attempt from ElasticSearch. " + response.ToString());
				return ServiceResult<IReadOnlyCollection<string>>.FailInternal();
			}
			return ReadSuggestionsResponse(response);
		}
		private ServiceResult<IReadOnlyCollection<string>> ReadSuggestionsResponse(SearchResponse<VideoSearchIndex> response/*, string suggestName*/)
		{
			var finalResult = response.Hits
				.Select(_ => _.Source!.TitleSuggestion).ToImmutableArray();
			return ServiceResult<IReadOnlyCollection<string>>.Success(finalResult);
		}
		private SortOptionsDescriptor<VideoSearchIndex>? BuildVideoSearchSort(VideoSortType sortType)
		{
			if (sortType == VideoSortType.Relevance)
				return null;
			var sort = new SortOptionsDescriptor<VideoSearchIndex>();
			if (sortType == VideoSortType.Time)
			{
				sort.Field(_ => _.UploadedAt, _ => _.Order(SortOrder.Desc));
				return sort;
			}
			if(sortType == VideoSortType.Views)
			{
				sort.Field(_ => _.Views, _ => _.Order(SortOrder.Desc));
				return sort;
			}
			//TODO: needs better rating system
			if(sortType == VideoSortType.Rating)
			{
				sort.Field(_ => _.Likes, _ => _.Order(SortOrder.Desc));
				return sort;
			}
			_logger.LogError($"Got unknown sort type {sortType}");
			return null;
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
					timeQuery = new DateRangeQuery(new Field("uploadedAt")) { From = now.AddDays(-1), To = now };
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
					{ From = options.ShortVideoSeconds, To = options.MediumVideoSeconds};
					break;
				case VideoLengthFilterType.Long:
					lengthQuery = new NumberRangeQuery(new Field("lengthSeconds"))
					{ From = options.MediumVideoSeconds};
					break;
			}
			return lengthQuery;
		}
	}
}
