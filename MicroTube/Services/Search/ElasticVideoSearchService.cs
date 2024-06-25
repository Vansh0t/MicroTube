using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using MicroTube.Data.Models;
using MicroTube.Services.ConfigOptions;

namespace MicroTube.Services.Search
{
	public class ElasticVideoSearchService : IVideoSearchService
	{
		private const string INDEX_NAME = "videos";

		private readonly IConfiguration _config;
		private readonly ElasticsearchClient _client;
		private readonly ILogger<ElasticVideoSearchService> _logger;

		public ElasticVideoSearchService(
			IConfiguration config,
			ElasticsearchClient client,
			ILogger<ElasticVideoSearchService> logger)
		{
			_config = config;
			_client = client;
			_logger = logger;
		}

		public async Task<IServiceResult<Video>> IndexVideo(Video video)
		{
			var videoIndexData = new VideoSearchIndexData(video.Id.ToString(), video.Title, video.Description);
			var indexResult = await _client.IndexAsync(videoIndexData, (IndexName)INDEX_NAME);
			if (!indexResult.IsValidResponse)
			{
				_logger.LogError("ElasticSearch indexing failed: " + indexResult.ToString());
				return ServiceResult<Video>.Fail(500, "ElasticSearch indexing failed: " + indexResult.Result);
			}
			_logger.LogInformation("ElasticSearch indexing done: " + indexResult.ToString());
			video.SearchIndexId = indexResult.Id;
			return ServiceResult<Video>.Success(video);
		}
		public async Task<IServiceResult<IReadOnlyCollection<VideoSearchIndexData>>> GetSuggestions(string input)
		{
			VideoSearchOptions options = _config.GetRequiredByKey<VideoSearchOptions>(VideoSearchOptions.KEY);
			var matchQueryTitle = new MatchQuery(new Field("Title"));
			var matchQueryDescription = new MatchQuery(new Field("Description"));
			var shouldQuery = new BoolQuery
			{
				MinimumShouldMatch = 1,
				Should = new Query[2] { matchQueryTitle, matchQueryDescription }
			};
			var result = await _client.SearchAsync<VideoSearchIndexData>(search =>
			{
				search.Index(INDEX_NAME)
				.From(0)
				.Size(options.MaxSuggestions)
				.Query(shouldQuery);
			});
			if (!result.IsValidResponse)
			{
				_logger.LogError("Failed to get suggestions attempt from ElasticSearch. " + result.ToString());
				return ServiceResult<IReadOnlyCollection<VideoSearchIndexData>>.FailInternal();
			}
			return ServiceResult<IReadOnlyCollection<VideoSearchIndexData>>.Success(result.Documents);
		}
	}
}
