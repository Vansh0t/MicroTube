using Elastic.Clients.Elasticsearch;
using Elastic.Transport.Products.Elasticsearch;
using MicroTube.Data.Models;
using MicroTube.Services.Validation;

namespace MicroTube.Services.Search
{
	public class ElasticsearchVideoSearchResultBuilder : IVideoSearchResultBuilder<SearchResponse<VideoSearchIndex>>
	{
		private readonly ILogger<ElasticsearchVideoSearchResultBuilder> _logger;
		private readonly ISearchMetaProvider<SearchResponse<VideoSearchIndex>, ElasticsearchMeta> _searchMetaProvider;
		private readonly ISearchResponseValidator<ElasticsearchResponse> _validator;
		public ElasticsearchVideoSearchResultBuilder(
			ILogger<ElasticsearchVideoSearchResultBuilder> logger,
			ISearchMetaProvider<SearchResponse<VideoSearchIndex>, ElasticsearchMeta> searchMetaProvider,
			ISearchResponseValidator<ElasticsearchResponse> validator)
		{
			_logger = logger;
			_searchMetaProvider = searchMetaProvider;
			_validator = validator;
		}

		public VideoSearchResult Build(SearchResponse<VideoSearchIndex> response, string? meta)
		{
			if (!_validator.Validate(response))
			{
				throw new ExternalServiceException("Failed to get suggestions attempt from ElasticSearch. " + response.ToString());
			}
			ElasticsearchMeta? searchMeta = _searchMetaProvider.BuildMeta(response);
			string? serializedSearchMeta = null;
			if (searchMeta != null)
			{
				serializedSearchMeta = _searchMetaProvider.SerializeMeta(searchMeta);
			}
			var result = new VideoSearchResult(response.Documents);
			if (response.Documents.Count == 0)
			{
				result.Meta = meta; //end of data reached, don't update meta to avoid loop
			}
			else
			{
				result.Meta = serializedSearchMeta;
			}
			return result;
		}
	}
}
