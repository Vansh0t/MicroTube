using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.Cryptography;

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
			var videoIndexData = new VideoSearchIndex(video.Id.ToString(), video.Title, video.Description);
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
		public async Task<IServiceResult> IndexSearchSuggestion(string text)
		{
			if(string.IsNullOrWhiteSpace(text))
			{
				return ServiceResult.Fail(400, "Bad input string, ignoring suggestion");
			}
			try
			{
				string hash = _md5Hash.HashAsString(text);
				VideoSearchSuggestionIndex? suggestionIndex = await _searchDataAccess.GetSuggestion(hash);
				if (suggestionIndex == null)
					suggestionIndex = new VideoSearchSuggestionIndex(hash, text, 1);
				await _searchDataAccess.IndexSuggestion(suggestionIndex);
			}
			catch (Exception e)
			{
				var error = $"Failed to index search suggestion {text}";
				_logger.LogError(e, $"Failed to index search suggestion {text}");
				return ServiceResult.Fail(500, error);
			}
			return ServiceResult.Success();
		}
		public async Task<IServiceResult<IReadOnlyCollection<VideoSearchIndex>>> GetVideos(string text)
		{
			VideoSearchOptions options = _config.GetRequiredByKey<VideoSearchOptions>(VideoSearchOptions.KEY);
			var matchQueryTitle = new MatchQuery(new Field("title")) { Query = text };
			var matchQueryDescription = new MatchQuery(new Field("description")) { Query = text };
			var shouldQuery = new BoolQuery
			{
				MinimumShouldMatch = 1,
				Should = new Query[2] { matchQueryTitle, matchQueryDescription }
			};
			var result = await _client.SearchAsync<VideoSearchIndex>(search =>
			{
				search.Index(options.VideosIndexName)
				.From(0)
				.Size(options.MaxSuggestions) //TO DO: Rework this
				.Query(shouldQuery);
			});
			if (!result.IsValidResponse)
			{
				_logger.LogError("Failed to get suggestions attempt from ElasticSearch. " + result.ToString());
				return ServiceResult<IReadOnlyCollection<VideoSearchIndex>>.FailInternal();
			}
			return ServiceResult<IReadOnlyCollection<VideoSearchIndex>>.Success(result.Documents);
		}
		public async Task<IServiceResult<IReadOnlyCollection<VideoSearchSuggestionIndex>>> GetSuggestions(string text)
		{
			VideoSearchOptions options = _config.GetRequiredByKey<VideoSearchOptions>(VideoSearchOptions.KEY);
			var matchQuery = new MatchQuery(new Field("Text")) { Query = text };
			var shouldQuery = new BoolQuery
			{
				MinimumShouldMatch = 1,
				Should = new Query[1] { matchQuery }
			};
			var result = await _client.SearchAsync<VideoSearchSuggestionIndex>(search =>
			{
				search.Index(options.SuggestionsIndexName)
				.From(0)
				.Size(options.MaxSuggestions)
				.Query(shouldQuery);
			});
			if (!result.IsValidResponse)
			{
				_logger.LogError("Failed to get suggestions attempt from ElasticSearch. " + result.ToString());
				return ServiceResult<IReadOnlyCollection<VideoSearchSuggestionIndex>>.FailInternal();
			}
			return ServiceResult<IReadOnlyCollection<VideoSearchSuggestionIndex>>.Success(result.Documents);
		}
	}
}
