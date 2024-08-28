using Elastic.Transport.Products.Elasticsearch;

namespace MicroTube.Services.Validation
{
	public class ElasticsearchResponseValidator : ISearchResponseValidator<ElasticsearchResponse>
	{
		public bool Validate(ElasticsearchResponse response)
		{
			//couldn't mock response object due to sealed/internal classes, so using this adaptor instead
			//maybe can be expanded in the future for more detailed logs
			return response.IsValidResponse;
		}
	}
}
