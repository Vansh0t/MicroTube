using Ardalis.GuardClauses;
using Azure.Storage.Blobs.Models;
using MicroTube.Services.ConfigOptions;
using System.IO.Abstractions;
using System.Threading;

namespace MicroTube.Services.ContentStorage
{
	public class AzureCdnMediaContentAccess : ICdnMediaContentAccess
	{
		private const int MAXIMUM_UPLOAD_CONCURRENCY = 8;

		private readonly IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> _videoRemoteStorage;
		private readonly IConfiguration _config;
		private readonly ILogger<AzureCdnMediaContentAccess> _logger;
		private readonly IFileSystem _fileSystem;

		public AzureCdnMediaContentAccess(
			IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> videoRemoteStorage,
			IConfiguration config,
			ILogger<AzureCdnMediaContentAccess> logger,
			IFileSystem fileSystem)
		{
			_videoRemoteStorage = videoRemoteStorage;
			_config = config;
			_logger = logger;
			_fileSystem = fileSystem;
		}

		public async Task<IServiceResult<IEnumerable<Uri>>> UploadVideoQualityTiers(string tiersDirectory, string remoteLocation, CancellationToken cancellationToken = default)
		{
			return await UploadPublicVideoContentFromDirectory(tiersDirectory, remoteLocation);
		}
		public async Task<IServiceResult<IEnumerable<Uri>>> UploadVideoThumbnails(string thumbnailsDirectory, string remoteLocation, CancellationToken cancellationToken =default)
		{
			return await UploadPublicVideoContentFromDirectory(thumbnailsDirectory, remoteLocation);
		}
		private async Task<IServiceResult<IEnumerable<Uri>>> UploadPublicVideoContentFromDirectory(string directory, string remoteLocation, CancellationToken cancellationToken = default)
		{
			try
			{
				Guard.Against.NullOrWhiteSpace(directory);
				Guard.Against.NullOrWhiteSpace(remoteLocation);
				VideoContentUploadOptions videoUploadOptions = _config.GetRequiredByKey<VideoContentUploadOptions>(VideoContentUploadOptions.KEY);
				var accessOptions = new AzureBlobAccessOptions("", remoteLocation);
				var uploadOptions = new BlobUploadOptions
				{
					AccessTier = AccessTier.Hot,
					TransferOptions = new Azure.Storage.StorageTransferOptions
					{
						MaximumConcurrency = MAXIMUM_UPLOAD_CONCURRENCY
					}
				};
				BulkUploadResult result = await _videoRemoteStorage.UploadDirectory(directory, accessOptions, uploadOptions, cancellationToken);
				var urls = result.SuccessfulUploadsFileNames.Select(_ => new Uri(videoUploadOptions.CdnUrl.JoinUrl($"{remoteLocation}/{_}")));
				if (result.SuccessfulUploadsFileNames.Count() == 0)
				{
					return ServiceResult<IEnumerable<Uri>>.Fail(500, $"No video content were uploaded to cdn from {directory} to {remoteLocation}");
				}
				else if (result.FailedUploadsFileNames.Count() > 0)
				{
					return new ServiceResult<IEnumerable<Uri>>(207, urls);
				}
				return ServiceResult<IEnumerable<Uri>>.Success(urls);
			}
			catch (ArgumentException e)
			{
				return ServiceResult<IEnumerable<Uri>>.Fail(400, e.ToString());
			}
			catch (Exception e)
			{
				_logger.LogError(e, $"Failed to upload video content tiers from path {directory} to remote location {remoteLocation}.");
				return ServiceResult<IEnumerable<Uri>>.Fail(500, $"Failed to upload video content tiers from path {directory} to remote location {remoteLocation}." + e.ToString());
			}
		}
	}
}
