using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using MicroTube.Data.Models;
using MicroTube.Services.Search.Videos;
using System.Text.Json;

namespace MicroTube.Services.Search
{
	public class ElasticsearchSearchMetaProvider : ISearchMetaProvider<SearchResponse<VideoSearchIndex>, ElasticsearchMeta>
	{
		public ElasticsearchMeta? BuildMeta(SearchResponse<VideoSearchIndex> data)
		{
			Hit<VideoSearchIndex>? lastHit = data.Hits.LastOrDefault();
			if (lastHit == null || lastHit.Sort == null)
			{
				return null;
			}
			var meta = new ElasticsearchMeta()
			{
				LastSort = lastHit.Sort
			};
			return meta;
		}

		public ElasticsearchMeta? DeserializeMeta(string? data)
		{
			if (data == null)
				return null;
			return JsonSerializer.Deserialize<ElasticsearchMeta>(data);
		}

		public string SerializeMeta(ElasticsearchMeta data)
		{
			return JsonSerializer.Serialize(data);
		}
	}
}
