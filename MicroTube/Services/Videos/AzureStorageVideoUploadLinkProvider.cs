using Ardalis.GuardClauses;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Elastic.Clients.Elasticsearch.Nodes;
using MicroTube.Controllers.Videos.Dto;
using MicroTube.Extensions;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.Validation;
using MicroTube.Services.VideoContent;
using System.IO.Abstractions;

namespace MicroTube.Services.Videos
{
	public class AzureStorageVideoUploadLinkProvider : IVideoUploadLinkProvider
	{
		private readonly IVideoFileNameGenerator _videoNameGenerator;
		private readonly IRemoteLocationNameGenerator _remoteLocationNameGenerator;
		private readonly BlobServiceClient _azureBlobServiceClient;
		private readonly IFileSystem _fileSystem;
		private readonly ILogger<AzureStorageVideoUploadLinkProvider> _logger;
		private readonly IConfiguration _config;
		private readonly IVideoPreUploadValidator _uploadValidator;
		public AzureStorageVideoUploadLinkProvider(
			IVideoFileNameGenerator videoNameGenerator,
			IRemoteLocationNameGenerator remoteLocationNameGenerator,
			BlobServiceClient azureBlobServiceClient,
			IFileSystem fileSystem,
			ILogger<AzureStorageVideoUploadLinkProvider> logger,
			IConfiguration config,
			IVideoPreUploadValidator validator)
		{
			_videoNameGenerator = videoNameGenerator;
			_remoteLocationNameGenerator = remoteLocationNameGenerator;
			_azureBlobServiceClient = azureBlobServiceClient;
			_fileSystem = fileSystem;
			_logger = logger;
			_config = config;
			_uploadValidator = validator;
		}

		public async Task<IServiceResult<VideoUploadLinkDto>> GetUploadLink(string fileName, Dictionary<string, string?> meta)
		{
			try
			{
				Guard.Against.NullOrWhiteSpace(fileName);
				var fileNameValidationResult = _uploadValidator.ValidateExtension(fileName);
				if(fileNameValidationResult.IsError)
				{
					return ServiceResult<VideoUploadLinkDto>.Fail(fileNameValidationResult.Code, fileNameValidationResult.Error!);
				}
				var options = _config.GetRequiredByKey<VideoContentUploadOptions>(VideoContentUploadOptions.KEY);
				string generatedFileName = _videoNameGenerator.GenerateVideoName() + _fileSystem.Path.GetExtension(fileName);
				string generatedRemoteLocationName = _remoteLocationNameGenerator.GetLocationName(generatedFileName);
				BlobContainerClient client = await CreateBlobContainer(generatedRemoteLocationName, meta);
				if (!client.CanGenerateSasUri)
				{
					throw new ExternalServiceException("Blob Container cannot generate SAS Url");
				}
				var sasBuilder = new BlobSasBuilder();
				sasBuilder.BlobContainerName = generatedRemoteLocationName;
				sasBuilder.SetPermissions(BlobAccountSasPermissions.Write | BlobAccountSasPermissions.Create);
				sasBuilder.ContentType = MimeMapping.MimeUtility.GetMimeMapping(fileName);
				sasBuilder.ExpiresOn = DateTime.UtcNow + TimeSpan.FromMinutes(options.DirectLinkLifetimeMinutes);
				sasBuilder.Resource = "b";
				var sasQuery = client.GenerateSasUri(sasBuilder).Query;
				var url = options.CdnUrl.UrlCombine(generatedRemoteLocationName, generatedFileName) + $"{sasQuery}";
				var dto = new VideoUploadLinkDto(url, generatedFileName, generatedRemoteLocationName);
				return ServiceResult<VideoUploadLinkDto>.Success(dto);
			}
			catch (ArgumentException)
			{
				return ServiceResult<VideoUploadLinkDto>.Fail(400, "Failed to generate an upload URL. Check the file name or title and try again.");
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to generate video upload link due to an unhandled exception.");
				return ServiceResult<VideoUploadLinkDto>.FailInternal();
			}
		}
		private async Task<BlobContainerClient> CreateBlobContainer(string containerName, Dictionary<string, string?> meta)
		{
			var client = _azureBlobServiceClient.GetBlobContainerClient(containerName);
			var createBlobContainerResult = await client.CreateIfNotExistsAsync(PublicAccessType.Blob, meta);
			if (createBlobContainerResult.GetRawResponse().IsError)
			{
				throw new ExternalServiceException($"Failed to create azure blob container for {containerName}. {createBlobContainerResult.GetRawResponse().ReasonPhrase}");
			}
			return client;
		}
	}
}
