﻿using Azure.Storage.Blobs;
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
using MicroTube.Services.VideoContent.Reactions;
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
			services.AddScoped<IVideoReactionsAggregator, DefaultVideoReactionsAggregator>();
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
		public static IServiceCollection AddAzureBlobStorage(this IServiceCollection services, string connectionString)
		{
			var blobServiceClient = new BlobServiceClient(connectionString);
			services.AddSingleton(blobServiceClient);
			return services;
		}
		public static IServiceCollection AddAzureCdnVideoPreprocessing(this IServiceCollection services)
		{
			services.AddScoped<VideoPreprocessingStage, UploadVideoSourceToRemoteStorageStage>();
			services.AddScoped<VideoPreprocessingStage, CreateUploadProgressStage>();
			services.AddScoped<VideoPreprocessingStage, MakeProcessingJobStage>();
			services.AddScoped<IVideoPreprocessingPipeline, DefaultVideoPreprocessingPipeline>();
			return services;
		}
		public static IServiceCollection AddAzureCdnVideoProcessing(this IServiceCollection services)
		{
			services.AddScoped<IVideoThumbnailsService, FFMpegVideoThumbnailsService>();
			services.AddScoped<IVideoCompressionService, FFMpegVideoCompressionService>();
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
			RecurringJob.AddOrUpdate<IVideoIndexingService>("VideoSearchIndexing", "video_indexing", service => service.EnsureVideoIndices(), Cron.Minutely);
			RecurringJob.AddOrUpdate<IVideoViewsAggregatorService>("VideoViewsAggregation", "video_views_aggregation", service => service.Aggregate(), Cron.Minutely);
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