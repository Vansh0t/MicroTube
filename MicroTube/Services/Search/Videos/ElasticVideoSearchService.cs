using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport.Products.Elasticsearch;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.Cryptography;
using MicroTube.Services.Validation;
using System.Collections.Immutable;

namespace MicroTube.Services.Search.Videos
{
	public class ElasticVideoSearchService : IVideoSearchService
	{

		private readonly IVideoSearchDataAccess _searchDataAccess;
		private readonly IConfiguration _config;
		private readonly ElasticsearchClient _client;
		private readonly ILogger<ElasticVideoSearchService> _logger;
		private readonly IVideoSearchRequestBuilder<SearchRequest<VideoSearchIndex>> _searchRequestBuilder;
		private readonly IVideoSearchResultBuilder<SearchResponse<VideoSearchIndex>> _searchResultBuilder;
		private readonly ISearchResponseValidator<ElasticsearchResponse> _responseValidator;

		public ElasticVideoSearchService(
			IConfiguration config,
			ElasticsearchClient client,
			ILogger<ElasticVideoSearchService> logger,
			IVideoSearchDataAccess searchDataAccess,
			IVideoSearchRequestBuilder<SearchRequest<VideoSearchIndex>> searchRequestBuilder,
			IVideoSearchResultBuilder<SearchResponse<VideoSearchIndex>> searchResultBuilder,
			ISearchResponseValidator<ElasticsearchResponse> responseValidator)
		{
			_config = config;
			_client = client;
			_logger = logger;
			_searchDataAccess = searchDataAccess;
			_searchRequestBuilder = searchRequestBuilder;
			_searchResultBuilder = searchResultBuilder;
			_responseValidator = responseValidator;
		}

		public async Task<IServiceResult<Video>> IndexVideo(Video video)
		{
			if (video.VideoViews == null || video.VideoReactions == null || video.VideoIndexing == null)
			{
				_logger.LogError($"Video {video.Id} does not have {nameof(video.VideoViews)}," +
					$" {nameof(video.VideoReactions)} or {nameof(video.VideoIndexing)}. Indexing failed.");
				return ServiceResult<Video>.FailInternal();
			}
			var videoIndexData = new VideoSearchIndex(video.Id.ToString(),
				video.Title,
				video.Description,
				video.Title,
				video.VideoViews.Views,
				video.VideoReactions.Likes,
				video.VideoReactions.Dislikes,
				video.LengthSeconds,
				video.UploadTime,
				video.UploaderId.ToString());
			try
			{
				string indexId = await _searchDataAccess.IndexVideo(videoIndexData);
				video.VideoIndexing.SearchIndexId = indexId;
				video.VideoIndexing.ReindexingRequired = false;
				video.VideoIndexing.LastIndexingTime = DateTime.UtcNow;
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
		public async Task<IServiceResult<VideoSearchResult>> GetVideos(VideoSearchParameters parameters, string? meta)
		{
			SearchRequest<VideoSearchIndex> searchRequest;
			try
			{
				searchRequest = _searchRequestBuilder.Build(parameters, meta);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to build videos search request");
				return ServiceResult<VideoSearchResult>.FailInternal();
			}
			SearchResponse<VideoSearchIndex> response = await _client.SearchAsync<VideoSearchIndex>(searchRequest);
			VideoSearchResult result;
			try
			{
				result = _searchResultBuilder.Build(response, meta);
				return ServiceResult<VideoSearchResult>.Success(result);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to build videos search result");
				return ServiceResult<VideoSearchResult>.FailInternal();
			}
		}
		public async Task<IServiceResult<IEnumerable<string>>> GetSuggestions(string text)
		{
			VideoSearchOptions options = _config.GetRequiredByKey<VideoSearchOptions>(VideoSearchOptions.KEY);
			MultiMatchQuery query = new MultiMatchQuery()
			{
				Query = text,
				Type = TextQueryType.BoolPrefix,
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
				.Query(query)
				.Size(options.MaxSuggestions)
				.Sort(sort => sort.Score(new ScoreSort() { Order = SortOrder.Desc }));
			});

			if (!_responseValidator.Validate(response))
			{
				_logger.LogError("Failed to get suggestions attempt from ElasticSearch. " + response.ToString());
				return ServiceResult<IEnumerable<string>>.FailInternal();
			}
			return ReadSuggestionsResponse(response);
		}
		private ServiceResult<IEnumerable<string>> ReadSuggestionsResponse(SearchResponse<VideoSearchIndex> response/*, string suggestName*/)
		{
			var finalResult = response.Hits
				.Select(_ => _.Source!.TitleSuggestion).ToImmutableArray();
			return ServiceResult<IEnumerable<string>>.Success(finalResult);
		}
	}
}
