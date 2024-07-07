using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
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
			var videoIndexData = new VideoSearchIndex(video.Id.ToString(), video.Title, video.Description, video.Title);
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
		public async Task<IServiceResult<IReadOnlyCollection<string>>> GetSuggestions(string text)
		{
			VideoSearchOptions options = _config.GetRequiredByKey<VideoSearchOptions>(VideoSearchOptions.KEY);
			MultiMatchQuery query = new MultiMatchQuery()
			{
				Query = text,
				Type = TextQueryType.PhrasePrefix,
				Analyzer = "keyword",
				Fields = Fields.FromStrings(new string[3]
				{
					"titleSuggestion",
					"titleSuggestion._2gram",
					"titleSuggestion._3gram"
				})
			};
			//string suggestName = "title_suggestiom";
			//var completion = new CompletionSuggester
			//{
			//	Field = "titleSuggestion",
			//	SkipDuplicates = true,
			//	Size = options.MaxSuggestions
			//};
			//var suggester = new Suggester()
			//{
			//	Text = text,
			//	Suggesters = new Dictionary<string, FieldSuggester>()
			//	{
			//		{ suggestName,  completion}
			//	}
			//};
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
			//if (response.Suggest == null)
			//{
			//	return ServiceResult<IReadOnlyCollection<string>>.Success(Array.Empty<string>());
			//}
			//var completionResult = response.Suggest.GetCompletion(suggestName);
			//if (completionResult == null)
			//{
			//	return ServiceResult<IReadOnlyCollection<string>>.Success(Array.Empty<string>());
			//}
			//var firstCompletion = completionResult.FirstOrDefault();
			//if (firstCompletion == null)
			//{
			//	return ServiceResult<IReadOnlyCollection<string>>.Success(Array.Empty<string>());
			//}
			//
			//var finalResult = firstCompletion.Options
			//	.Where(_=>_.Source != null)
			//	.Select(_ => _.Source!.TitleSuggestion).ToImmutableArray();
			//return ServiceResult<IReadOnlyCollection<string>>.Success(finalResult);
			var finalResult = response.Hits
				.Select(_ => _.Source!.TitleSuggestion).ToImmutableArray();
			return ServiceResult<IReadOnlyCollection<string>>.Success(finalResult);
		}
	}
}
