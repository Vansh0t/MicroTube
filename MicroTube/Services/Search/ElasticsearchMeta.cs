using Elastic.Clients.Elasticsearch;

namespace MicroTube.Services.Search
{
	public class ElasticsearchMeta
	{
		public IEnumerable<FieldValue>? LastSort { get; set; }
	}
}
