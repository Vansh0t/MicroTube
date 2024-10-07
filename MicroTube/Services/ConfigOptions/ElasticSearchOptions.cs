namespace MicroTube.Services.ConfigOptions
{
	public class ElasticSearchOptions
	{
		public const string KEY = "Elasticsearch";

		public string ApiKey { get; set; }
		public string Url { get; set; }
		public ElasticSearchOptions(string apiKey, string url)
		{
			ApiKey = apiKey;
			Url = url;
		}
	}
}
