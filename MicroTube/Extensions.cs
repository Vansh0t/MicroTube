using Azure.Storage.Blobs;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Transport;
using MicroTube.Data.Models;
using MicroTube.Services;
using MicroTube.Services.Authentication;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.Cryptography;
using MicroTube.Services.VideoContent.Processing;
using MicroTube.Services.VideoContent.Processing.Stages;
using MicroTube.Services.VideoContent.Processing.Stages.Offline;

namespace MicroTube
{
    //TODO: Needs refactoring.
    public static class Extensions
    {
        private const string DEFAULT_CONNECTION_STRING_NAME = "ConnectionStrings:Default";
        public static string GetDefaultConnectionString(this IConfiguration configuration)
        {
            string? defaultConnectionString = configuration[DEFAULT_CONNECTION_STRING_NAME];
            if (defaultConnectionString is null)
                throw new ConfigurationException("Configuration does not contain connection string " + DEFAULT_CONNECTION_STRING_NAME);
            return defaultConnectionString;
        }
        public static T GetRequiredByKey<T>(this IConfiguration configuration, string sectionKey)
        {
            T? result = configuration.GetRequiredSection(sectionKey).Get<T>();
            if (result == null)
                throw new ConfigurationException($"Unable to bind configuration section {sectionKey} to {typeof(T)}");
            return result;
        }
		public static string GetRequiredValue(this IConfiguration configuration, string path)
		{
			string? value = configuration[path];
			if(value == null)
				throw new ConfigurationException($"Required value is null. Path: {path}");
			return value;
		}
		public static string GetAppRootPath(this IConfiguration configuration)
		{
			string? value = configuration.GetValue<string>(WebHostDefaults.ContentRootKey);
			if (value == null)
				throw new ConfigurationException($"WebHostDefaults.ContentRootKey value is null.");
			return value;
		}
		public static IServiceResult<string> BuildJWTAccessToken(this IJwtTokenProvider tokenProvider, IJwtClaims claims, AppUser user)
		{
			return tokenProvider.GetToken(claims.GetClaims(user));
		}
		public static T GetRequiredObject<T>(this IServiceResult<T> result)
		{
			if (result.IsError)
				throw new RequiredObjectNotFoundException($"{nameof(IServiceResult<T>)} for must not be and error.");
			if (result.ResultObject == null)
				throw new RequiredObjectNotFoundException($"A successful {nameof(IServiceResult<T>)} must always provide ResultObject.");
			return result.ResultObject;
		}
		public static void AddRefreshTokenCookie(this HttpContext context, IConfiguration config, string refreshToken, DateTime expiration)
		{
			string path = config.GetRequiredByKey<string>("UserSession:RefreshPath");
			string domain = config.GetRequiredValue("ClientApp:Domain");
			CookieOptions options = new CookieOptions
			{
				Expires = expiration,
				Domain = domain,
				Path = path,
				HttpOnly = true,
				SameSite = SameSiteMode.None,
				Secure = true
			};
			context.Response.Cookies.Append(Constants.AuthorizationConstants.REFRESH_TOKEN_COOKIE_KEY, refreshToken, options);
		}
		public static T GetRequired<T>(this IConfigurationSection section) where T : class
		{
			T? result = section.Get<T>();
			if (result == null)
			{
				throw new ConfigurationException($"Unable to find and map a required configuration section of type {typeof(T)}");
			}
			return result;
		}
		public static IServiceCollection AddAzureBlobStorage(this IServiceCollection services, string connectionString)
		{
			var blobServiceClient = new BlobServiceClient(connectionString);
			services.AddSingleton(blobServiceClient);
			return services;
		}
		public static IServiceResult ExceptionToErrorResult(this Exception exception, string prependInfo = "", int code = 500)
		{
			var result = ServiceResult.Fail(code, $"{prependInfo}. {exception}");
			return result;
		}
		public static IServiceResult<T> ExceptionToErrorResult<T>(
			this Exception exception,
			string prependInfo = "",
			int code = 500)
		{
			var result = ServiceResult<T>.Fail(code, $"{prependInfo}. {exception}");
			return result;
		}
		public static IServiceCollection AddElasticSearchClient(this IServiceCollection services, IConfiguration config)
		{
			var options = config.GetRequiredByKey<ElasticSearchOptions>(ElasticSearchOptions.KEY);
			var nodesPool = new SingleNodePool(new Uri(options.Url));
			var apiKey = new ApiKey(options.ApiKey);
			var clientSettings = new ElasticsearchClientSettings(nodesPool)
				.Authentication(apiKey);
			var elasticSearchClient = new ElasticsearchClient(clientSettings);
			EnsureElasticsearchIndices(elasticSearchClient, config);
			services.AddSingleton(elasticSearchClient);
			return services;
		}
		public static IServiceCollection AddOfflineVideoProcessing(this IServiceCollection services)
		{
			services.AddScoped<VideoProcessingStage, FetchVideoUploadProgressStage>();
			services.AddScoped<VideoProcessingStage, OfflineFetchVideoSourceFromRemoteCacheStage>();
			services.AddScoped<VideoProcessingStage, SetProgressInProgressStage>();
			services.AddScoped<VideoProcessingStage, FFMpegCreateQualityTiersStage>();
			services.AddScoped<VideoProcessingStage, FFMpegCreateThumbnailsStage>();
			services.AddScoped<VideoProcessingStage, OfflineUploadThumbnailsToCdnStage>();
			services.AddScoped<VideoProcessingStage, OfflineUploadVideoToCdnStage>();
			services.AddScoped<VideoProcessingStage, CreateVideoInDatabaseStage>();
			services.AddScoped<VideoProcessingStage, SetProgressFinishedStage>();
			services.AddScoped<IVideoProcessingPipeline, OfflineVideoProcessingPipeline>();
			return services;
		}
		private static void EnsureElasticsearchIndices(ElasticsearchClient client, IConfiguration config)
		{
			var options = config.GetRequiredByKey<VideoSearchOptions>(VideoSearchOptions.KEY);
			var analysisSettings = new IndexSettingsAnalysis();
			analysisSettings.TokenFilters = new TokenFilters
			{
				{"lowercase", new LowercaseTokenFilter() },
				{"edge_ngram", new EdgeNGramTokenFilter() { MinGram = 2, MaxGram = 5, PreserveOriginal= true} }
			};
			var nGramAnalyzer = new CustomAnalyzer()
			{
				Tokenizer = "standard",
				Filter = new string[2] { "lowercase", "edge_ngram" }
			};
			analysisSettings.Analyzers = new Analyzers
			{
				{ "ngram_analyzer", nGramAnalyzer }
			};
			TypeMapping mapping = new TypeMapping()
			{
				Properties = new Properties
				{
					{"title", new TextProperty() { Analyzer = "ngram_analyzer"} },
					{"description", new TextProperty() { Analyzer = "standard"} },
					{"titleSuggestion", new SearchAsYouTypeProperty() }
				},
				
			};
			var indexSettings = new IndexSettings()
			{
				Analysis = analysisSettings,
			};
			var createResult = client.Indices.Create((IndexName)options.VideosIndexName, 
				_ => {
					_.Settings(indexSettings);
					_.Mappings(mapping);
					}) ;
			//var settingsResult = client.Indices.PutSettings(indexSettings, (IndexName)options.VideosIndexName);
			Console.WriteLine(createResult.DebugInformation);


			//Console.WriteLine(settingsResult.DebugInformation);
			
		}
	}
}
