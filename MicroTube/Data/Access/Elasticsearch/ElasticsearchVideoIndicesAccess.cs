using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using MicroTube.Data.Models.Videos;
using MicroTube.Services.ConfigOptions;

namespace MicroTube.Data.Access.Elasticsearch
{
    public class ElasticsearchVideoIndicesAccess : IVideoSearchDataAccess
	{
		private readonly ElasticsearchClient _client;
		private readonly IConfiguration _config;
		private readonly ILogger<ElasticsearchVideoIndicesAccess> _logger;
		

		public ElasticsearchVideoIndicesAccess(
			ElasticsearchClient client,
			IConfiguration config,
			ILogger<ElasticsearchVideoIndicesAccess> logger)
		{
			_client = client;
			_config = config;
			_logger = logger;
		}

		public async Task<VideoSearchSuggestionIndex?> GetSuggestion(string text)
		{
			var options = _config.GetRequiredByKey<VideoSearchOptions>(VideoSearchOptions.KEY);
			var request = new SearchRequest(options.SuggestionsIndexName)
			{
				From = 0,
				Size = 1,
				Query = new TermQuery("Text") { Value = text }
			};
			var response = await _client.SearchAsync<VideoSearchSuggestionIndex?>(request);
			if (!response.IsValidResponse)
			{
				throw new DataAccessException("Failed to get video search suggestion from Elasticsearch. " + response.ToString());
			}
			return response.Documents.FirstOrDefault();
		}
		public async Task<string> IndexSuggestion(VideoSearchSuggestionIndex suggestionIndex)
		{
			var options = _config.GetRequiredByKey<VideoSearchOptions>(VideoSearchOptions.KEY);
			var indexResult = await _client.IndexAsync(suggestionIndex, (IndexName)options.SuggestionsIndexName);
			if (!indexResult.IsValidResponse)
			{
				throw new DataAccessException("Failed to index suggestion with Elasticsearch: " + indexResult.ToString());
			}
			return indexResult.Id;
		}
		public async Task<string> IndexVideo(VideoSearchIndex videoIndex)
		{
			var options = _config.GetRequiredByKey<VideoSearchOptions>(VideoSearchOptions.KEY);
			IndexRequest<VideoSearchIndex> request = new IndexRequest<VideoSearchIndex>(videoIndex, (IndexName)options.VideosIndexName);
			var indexResult = await _client.IndexAsync(request);
			if (!indexResult.IsValidResponse)
			{
				throw new DataAccessException("Failed to index suggestion with Elasticsearch: " + indexResult.ToString());
			}
			return indexResult.Id;
		}
		public async Task<VideoSearchIndex?> GetVideoIndex(string id)
		{
			var options = _config.GetRequiredByKey<VideoSearchOptions>(VideoSearchOptions.KEY);
			GetRequest request = new GetRequest((IndexName)options.VideosIndexName, id);
			GetResponse<VideoSearchIndex> response = await _client.GetAsync<VideoSearchIndex>(request);
			if (!response.IsValidResponse)
			{
				throw new DataAccessException("Failed to index suggestion with Elasticsearch: " + response.ToString());
			}
			return response.Source;
		}
	}
}
