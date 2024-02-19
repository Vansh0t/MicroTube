using Azure.Storage.Blobs.Models;
using MicroTube.Services.ConfigOptions;

namespace MicroTube.Services.MediaContentStorage
{
	public class AzureCdnMediaContentAccess : ICdnMediaContentAccess
	{
		private const int MAXIMUM_UPLOAD_CONCURRENCY = 8;

		private readonly IVideoContentRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> _videoRemoteStorage;
		private readonly IConfiguration _config;
		private readonly ILogger<AzureCdnMediaContentAccess> _logger;

		public AzureCdnMediaContentAccess(IVideoContentRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> videoRemoteStorage, IConfiguration config, ILogger<AzureCdnMediaContentAccess> logger)
		{
			_videoRemoteStorage = videoRemoteStorage;
			_config = config;
			_logger = logger;
		}

		public async Task<IServiceResult<Uri>> UploadVideo(Stream stream, string fileName, CancellationToken cancellationToken = default)
		{
			VideoContentUploadOptions videoUploadOptions = _config.GetRequiredByKey<VideoContentUploadOptions>(VideoContentUploadOptions.KEY);
			var accessOptions = new AzureBlobAccessOptions(fileName, videoUploadOptions.RemoteStorageLocation);
			var uploadOptions = new BlobUploadOptions
			{
				AccessTier = AccessTier.Hot
			};
			var uploadResult = await _videoRemoteStorage.Upload(stream, accessOptions, uploadOptions, cancellationToken);
			if(uploadResult.IsError)
			{
				_logger.LogError("Failed to upload video {videoFileName}.", fileName);
				return ServiceResult<Uri>.Fail(uploadResult.Code, uploadResult.Error!);
			}
			var cdnUri = new Uri(videoUploadOptions.CdnUrl);
			var videoUri = new Uri(cdnUri, $"{videoUploadOptions.RemoteStorageLocation}/{fileName}");
			return ServiceResult<Uri>.Success(videoUri);
		}
		public async Task<IServiceResult<IEnumerable<Uri>>> UploadVideoSubcontent(string fromPath, string videoFileName, CancellationToken cancellationToken =default)
		{
			VideoContentUploadOptions videoUploadOptions = _config.GetRequiredByKey<VideoContentUploadOptions>(VideoContentUploadOptions.KEY);
			string videoFileNameWithoutExtension = videoFileName.Replace(Path.GetExtension(videoFileName), "");
			var accessOptions = new AzureBlobAccessOptions("", videoFileNameWithoutExtension);
			var uploadOptions = new BlobUploadOptions
			{
				AccessTier = AccessTier.Hot,
				TransferOptions = new Azure.Storage.StorageTransferOptions
				{
					MaximumConcurrency = MAXIMUM_UPLOAD_CONCURRENCY
				}
			};
			var ensureLocationResult = await _videoRemoteStorage.EnsureLocation(videoFileNameWithoutExtension, RemoteLocationAccess.Public, cancellationToken);
			if(ensureLocationResult.IsError)
			{
				_logger.LogError("Failed to ensure video subcontent location {fromPath}, {videoFileName}.", fromPath, videoFileName);
				return ServiceResult<IEnumerable<Uri>>.Fail(ensureLocationResult.Code, ensureLocationResult.Error!);
			}
			var uploadResult = await _videoRemoteStorage.Upload(fromPath, accessOptions, uploadOptions, cancellationToken);
			if (uploadResult.IsError)
			{
				_logger.LogError("Failed to upload video subcontent {fromPath}, {videoFileName}.", fromPath, videoFileName);
				return ServiceResult<IEnumerable<Uri>>.Fail(uploadResult.Code, uploadResult.Error!);
			}
			var uploadedFileNames = uploadResult.GetRequiredObject();
			var cdnUri = new Uri( videoUploadOptions.CdnUrl);
			var uploadedFilesUris = uploadedFileNames.Select(_ => new Uri(cdnUri, $"{videoFileNameWithoutExtension}/{_}"));
			return ServiceResult<IEnumerable<Uri>>.Success(uploadedFilesUris);
		}
		public async Task<IServiceResult> DeleteAllVideoData(string videoFileName, CancellationToken cancellationToken = default)
		{
			VideoContentUploadOptions videoUploadOptions = _config.GetRequiredByKey<VideoContentUploadOptions>(VideoContentUploadOptions.KEY);
			var accessOptions = new AzureBlobAccessOptions(videoFileName, videoUploadOptions.RemoteStorageLocation);
			var deleteVideoTask =  _videoRemoteStorage.Delete(accessOptions, cancellationToken);
			var deleteSubcontentTask = _videoRemoteStorage.DeleteLocation(Path.GetFileNameWithoutExtension(videoFileName));
			await Task.WhenAll(deleteVideoTask, deleteSubcontentTask);
			var deleteVideoResult = deleteVideoTask.Result;
			var deleteSubcontentResult = deleteSubcontentTask.Result;
			string joinedError = "" + deleteVideoResult.Error + deleteSubcontentResult.Error;
			if (!string.IsNullOrWhiteSpace(joinedError))
			{
				return ServiceResult.Fail(500, joinedError);
			}
			return ServiceResult.Success();
		}
	}
}
