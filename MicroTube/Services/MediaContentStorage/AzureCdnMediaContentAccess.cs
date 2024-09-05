using Ardalis.GuardClauses;
using Azure.Storage.Blobs.Models;
using MicroTube.Services.ConfigOptions;
using System.IO.Abstractions;
using System.Threading;

namespace MicroTube.Services.MediaContentStorage
{
	public class AzureCdnMediaContentAccess : ICdnMediaContentAccess
	{
		private const int MAXIMUM_UPLOAD_CONCURRENCY = 8;

		private readonly IVideoContentRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> _videoRemoteStorage;
		private readonly IConfiguration _config;
		private readonly ILogger<AzureCdnMediaContentAccess> _logger;
		private readonly IFileSystem _fileSystem;

		public AzureCdnMediaContentAccess(
			IVideoContentRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> videoRemoteStorage,
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
		public async Task<IServiceResult<string>> CreateVideoLocation(string videoFileName, CancellationToken cancellationToken = default)
		{
			try
			{
				Guard.Against.NullOrWhiteSpace(videoFileName);
				string fileNameWithoutExtension = BuildRemoteLocationNameFromVideoFileName(videoFileName);
				await _videoRemoteStorage.EnsureLocation(fileNameWithoutExtension, RemoteLocationAccess.Public, cancellationToken);
				return ServiceResult<string>.Success(fileNameWithoutExtension);
			}
			catch(ArgumentException e)
			{
				return ServiceResult<string>.Fail(400, e.ToString());
			}
			catch(Exception e)
			{
				_logger.LogError(e, $"Failed to create video location {videoFileName}.");
				return ServiceResult<string>.Fail(500, $"Failed to create video location {videoFileName}. " + e.ToString());
			}
		}
		public async Task<IServiceResult> DeleteAllVideoData(string videoFileName, CancellationToken cancellationToken = default)
		{
			try
			{
				Guard.Against.NullOrWhiteSpace(videoFileName);
				await _videoRemoteStorage.DeleteLocation(BuildRemoteLocationNameFromVideoFileName(videoFileName));
				return ServiceResult.Success();
			}
			catch (ArgumentException e)
			{
				return ServiceResult<string>.Fail(400, e.ToString());
			}
			catch (Exception e)
			{
				_logger.LogError(e, $"Failed to delete remote video location for video file {videoFileName}");
				return ServiceResult.Fail(500, $"Failed to delete remote video location for video file {videoFileName}. " + e);
			}
			
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
				var cdnUrl = new Uri(videoUploadOptions.CdnUrl);
				var urls = result.SuccessfulUploadsFileNames.Select(_ => new Uri(cdnUrl, $"{remoteLocation}/{_}"));
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
		private string BuildRemoteLocationNameFromVideoFileName(string fileName)
		{
			return _fileSystem.Path.GetFileNameWithoutExtension(fileName);
		}
	}
}
