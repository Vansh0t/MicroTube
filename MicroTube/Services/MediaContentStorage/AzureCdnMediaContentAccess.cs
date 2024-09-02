using Azure.Storage.Blobs.Models;
using MicroTube.Services.ConfigOptions;
using System.IO.Abstractions;

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

		public async Task<IServiceResult<Uri>> UploadVideo(Stream stream, string fileName, string location, CancellationToken cancellationToken = default)
		{
			VideoContentUploadOptions videoUploadOptions = _config.GetRequiredByKey<VideoContentUploadOptions>(VideoContentUploadOptions.KEY);
			var accessOptions = new AzureBlobAccessOptions(fileName, location);
			var uploadOptions = new BlobUploadOptions
			{
				AccessTier = AccessTier.Hot
			};
			
			try
			{
				string uploadedFileName = await _videoRemoteStorage.Upload(stream, accessOptions, uploadOptions, cancellationToken);
				var cdnUrl = new Uri(videoUploadOptions.CdnUrl);
				var videoUrl = new Uri(cdnUrl, $"{location}/{uploadedFileName}");
				return ServiceResult<Uri>.Success(videoUrl);
			}
			catch (Exception e)
			{
				_logger.LogError(e, $"Failed to upload video {fileName}.");
				return ServiceResult<Uri>.Fail(500, $"Failed to upload video {fileName}. " + e.ToString());
			}
		}
		public async Task<IServiceResult<IEnumerable<Uri>>> UploadVideoSubcontent(string fromPath, string location, CancellationToken cancellationToken =default)
		{
			VideoContentUploadOptions videoUploadOptions = _config.GetRequiredByKey<VideoContentUploadOptions>(VideoContentUploadOptions.KEY);
			var accessOptions = new AzureBlobAccessOptions("", location);
			var uploadOptions = new BlobUploadOptions
			{
				AccessTier = AccessTier.Hot,
				TransferOptions = new Azure.Storage.StorageTransferOptions
				{
					MaximumConcurrency = MAXIMUM_UPLOAD_CONCURRENCY
				}
			};
			try
			{
				var result = await _videoRemoteStorage.Upload(fromPath, accessOptions, uploadOptions, cancellationToken);
				var cdnUrl = new Uri(videoUploadOptions.CdnUrl);
				var uploadedFilesUrls = result.Select(_ => new Uri(cdnUrl, $"{location}/{_}"));
				return ServiceResult<IEnumerable<Uri>>.Success(uploadedFilesUrls);
			}
			catch (Exception e)
			{
				_logger.LogError(e, $"Failed to upload video subcontent {fromPath}, {location}.");
				return ServiceResult<IEnumerable<Uri>>.Fail(500, $"Failed to upload video subcontent {fromPath}, {location}. " + e.ToString());
			}
		}
		public async Task<IServiceResult<string>> CreateVideoLocation(string videoFileName, CancellationToken cancellationToken = default)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(videoFileName);
			try
			{
				await _videoRemoteStorage.EnsureLocation(fileNameWithoutExtension, RemoteLocationAccess.Public, cancellationToken);
				return ServiceResult<string>.Success(fileNameWithoutExtension);
			}
			catch(Exception e)
			{
				_logger.LogError(e, $"Failed to create video location {videoFileName}.");
				return ServiceResult<string>.Fail(500, $"Failed to create video location {videoFileName}. " + e.ToString());
			}
		}
		public async Task<IServiceResult> DeleteAllVideoData(string videoFileName, CancellationToken cancellationToken = default)
		{
			VideoContentUploadOptions videoUploadOptions = _config.GetRequiredByKey<VideoContentUploadOptions>(VideoContentUploadOptions.KEY);
			var accessOptions = new AzureBlobAccessOptions(videoFileName, videoUploadOptions.RemoteStorageLocation);
			try
			{
				await _videoRemoteStorage.DeleteLocation(Path.GetFileNameWithoutExtension(videoFileName));
				return ServiceResult.Success();
			}
			catch (Exception e)
			{
				_logger.LogError(e, $"Failed to delete remote video location for video file {videoFileName}");
				return ServiceResult.Fail(500, $"Failed to delete remote video location for video file {videoFileName}. " + e);
			}
			
		}
	}
}
