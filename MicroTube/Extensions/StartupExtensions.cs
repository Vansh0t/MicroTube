using Azure.Storage.Blobs;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport.Products.Elasticsearch;
using Elastic.Transport;
using MicroTube.Data.Access.Elasticsearch;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.Authentication.BasicFlow;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.Search;
using MicroTube.Services.Validation;
using MicroTube.Services.VideoContent.Likes;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using MicroTube.Services.VideoContent.Processing.Stages;
using MicroTube.Services.VideoContent.Processing.Stages.Azure;
using MicroTube.Services.VideoContent.Processing;
using MicroTube.Services.VideoContent.Preprocessing.Stages;
using MicroTube.Services.VideoContent.Preprocessing;
using Hangfire;
using MicroTube.Services.VideoContent.Views;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using MicroTube.Services.ContentStorage;
using Azure.Storage.Blobs.Models;
using MicroTube.Services.VideoContent;
using MicroTube.Constants;
using MicroTube.Services.HangfireFilters;
using MicroTube.Services.Authentication;
using Hangfire.Dashboard;
using MicroTube.Services.Reactions;

namespace MicroTube.Extensions
{
    public static class StartupExtensions
	{
		public static IServiceCollection AddDefaultBasicAuthenticationFlow(this IServiceCollection services)
		{
			services.AddScoped<IBasicFlowAuthenticationProvider, DefaultBasicFlowAuthenticationProvider>();
			services.AddScoped<IBasicFlowEmailHandler, DefaultBasicFlowEmailHandler>();
			services.AddScoped<IBasicFlowPasswordHandler, DefaultBasicFlowPasswordHandler>();
			return services;
		}
		public static IServiceCollection AddVideoReactions(this IServiceCollection services)
		{
			services.AddScoped<ILikeDislikeReactionAggregator, LikeDislikeReactionAggregator>();
			services.AddScoped<IVideoReactionsService, DefaultVideoReactionsService>();
			return services;
		}
		public static IServiceCollection AddElasticsearchClient(this IServiceCollection services, IConfiguration config)
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
		public static IServiceCollection AddElasticsearchSearch(this IServiceCollection services)
		{
			services.AddScoped<IVideoSearchDataAccess, ElasticsearchVideoIndicesAccess>();
			services.AddTransient<IVideoSearchRequestBuilder<SearchRequest<VideoSearchIndex>>, ElasticsearchVideoSearchRequestBuilder>();
			services.AddTransient<IVideoSearchResultBuilder<SearchResponse<VideoSearchIndex>>, ElasticsearchVideoSearchResultBuilder>();
			services.AddTransient<ISearchResponseValidator<ElasticsearchResponse>, ElasticsearchResponseValidator>();
			services.AddScoped<ISearchMetaProvider<SearchResponse<VideoSearchIndex>, ElasticsearchMeta>, ElasticsearchSearchMetaProvider>();
			services.AddScoped<IVideoSearchService, ElasticVideoSearchService>();
			return services;
		}
		public static IServiceCollection AddAzureBlobRemoteStorage(this IServiceCollection services, string connectionString)
		{
			var blobServiceClient = new BlobServiceClient(connectionString);
			services.AddSingleton(blobServiceClient);
			services.AddScoped<IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions>, AzureBlobContentStorage>();
			return services;
		}
		public static IServiceCollection AddAzureCdnVideoPreprocessing(this IServiceCollection services)
		{
			services.AddTransient<IVideoFileNameGenerator, GuidVideoFileNameGenerator>();
			services.AddTransient<IRemoteLocationNameGenerator, ExtensionlessFileNameRemoteLocationNameGenerator>();
			services.AddScoped<VideoPreprocessingStage, UploadVideoSourceToRemoteStorageStage>();
			services.AddScoped<VideoPreprocessingStage, CreateUploadProgressStage>();
			services.AddScoped<VideoPreprocessingStage, MakeProcessingJobStage>();
			services.AddScoped<IVideoPreprocessingPipeline, DefaultVideoPreprocessingPipeline>();
			return services;
		}
		public static IServiceCollection AddAzureCdnVideoProcessing(this IServiceCollection services)
		{
			services.AddScoped<ICdnMediaContentAccess, AzureCdnMediaContentAccess>();
			services.AddScoped<IVideoThumbnailsService, FFMpegVideoThumbnailsService>();
			services.AddScoped<IVideoCompressionService, FFMpegVideoCompressionService>();
			services.AddScoped<IVideoProcessingArgumentsProvider, FFMpegVideoProcessingArgumentsProvider>();
			services.AddScoped<VideoProcessingStage, FetchVideoUploadProgressStage>();
			services.AddScoped<VideoProcessingStage, AzureFetchVideoSourceFromRemoteCacheStage>();
			services.AddScoped<VideoProcessingStage, SetProgressInProgressStage>();
			services.AddScoped<VideoProcessingStage, FFMpegCreateQualityTiersStage>();
			services.AddScoped<VideoProcessingStage, FFMpegCreateThumbnailsStage>();
			services.AddScoped<VideoProcessingStage, AzureUploadThumbnailsToCdnStage>();
			services.AddScoped<VideoProcessingStage, AzureUploadVideoToCdnStage>();
			services.AddScoped<VideoProcessingStage, CreateVideoInDatabaseStage>();
			services.AddScoped<VideoProcessingStage, SetProgressFinishedStage>();
			services.AddScoped<IVideoProcessingPipeline, DefaultVideoProcessingPipeline>();
			return services;
		}
		public static void ScheduleBackgroundJobs()
		{
			RecurringJob.AddOrUpdate<IVideoIndexingService>("VideoSearchIndexing", HangfireConstants.VIDEO_INDEXING_QUEUE, service => service.EnsureVideoIndices(), Cron.Minutely);
			RecurringJob.AddOrUpdate<IVideoViewsAggregatorService>("VideoViewsAggregation", HangfireConstants.VIDEO_VIEWS_AGGREGATION_QUEUE, service => service.Aggregate(), Cron.Minutely);
		}
		public static void EnsureDatabaseCreated(string connectionString)
		{
			Guard.Against.NullOrWhiteSpace(connectionString);
			var dbOptionsBuilder = new DbContextOptionsBuilder<MicroTubeDbContext>();
			dbOptionsBuilder.UseSqlServer(connectionString);
			using var dbContext = new MicroTubeDbContext(dbOptionsBuilder.Options);
			dbContext.Database.EnsureCreated();
		}
		public static IServiceCollection AddHangfireClient(this IServiceCollection services, IConfiguration config)
		{
			services.AddHangfire((serviceProvider, hangfireConfig) =>
			{
				hangfireConfig.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
				.UseSimpleAssemblyNameTypeSerializer()
				.UseRecommendedSerializerSettings()
				.UseSqlServerStorage(config.GetDefaultConnectionString())
				.UseColouredConsoleLogProvider()
				.UseFilter(new AutomaticRetryAttribute { Attempts = 3 })
				.UseFilter(new CleanupVideoProcessingJobHangfireFilter(serviceProvider));

			});
			return services;
		}
		public static IServiceCollection AddHangfireServers(this IServiceCollection services)
		{
			services.AddHangfireServer((provider, options) =>
			{
				options.ServerName = "VideoProcessing_1";
				options.WorkerCount = 1;
				options.Queues = new[] { HangfireConstants.VIDEO_PROCESSING_QUEUE };
			});
			services.AddHangfireServer((options) =>
			{
				options.ServerName = "VideoMetaProcessing_1";
				options.WorkerCount = 1;
				options.Queues = new[] { HangfireConstants.VIDEO_INDEXING_QUEUE, HangfireConstants.VIDEO_VIEWS_AGGREGATION_QUEUE };
			});
			return services;
		}
		public static WebApplication UseHangfireDashboard(this WebApplication app)
		{
			IDashboardAuthorizationFilter authFilter;
			if(app.Environment.IsDevelopment())
			{
				authFilter = new HangfireDashboardAnonymousAuthorizationFilter();
			}
			else
			{
				authFilter = new AdminHangfireDashboardAuthorizationFilter();
			}
			app.MapHangfireDashboard(
				new DashboardOptions()
				{
					Authorization = new[] { authFilter }
				});
			return app;
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
				});
			Console.WriteLine(createResult.DebugInformation);
		}
	}
}
