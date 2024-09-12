using Azure.Storage.Blobs.Models;
using MicroTube.Services.ConfigOptions;
using System.IO.Abstractions;

namespace MicroTube.Services.MediaContentStorage
{
	public class OfflineCdnMediaContentAccess : ICdnMediaContentAccess
	{
		private const int MAXIMUM_UPLOAD_CONCURRENCY = 8;

		private const string OFFLINE_CDN_LOCATION = "/data/";
		private readonly IVideoContentRemoteStorage<OfflineRemoteStorageOptions, OfflineRemoteStorageOptions> _videoRemoteStorage;
		private readonly IConfiguration _config;
		private readonly ILogger<OfflineCdnMediaContentAccess> _logger;
		private readonly IFileSystem _fileSystem;
		public OfflineCdnMediaContentAccess(
			IVideoContentRemoteStorage<OfflineRemoteStorageOptions, OfflineRemoteStorageOptions> videoRemoteStorage,
			IConfiguration config,
			ILogger<OfflineCdnMediaContentAccess> logger,
			IFileSystem fileSystem)
		{
			_videoRemoteStorage = videoRemoteStorage;
			_config = config;
			_logger = logger;
			_fileSystem = fileSystem;
		}

		public async Task<IServiceResult<Uri>> UploadVideo(Stream stream, string fileName, CancellationToken cancellationToken = default)
		{
			VideoContentUploadOptions videoUploadOptions = _config.GetRequiredByKey<VideoContentUploadOptions>(VideoContentUploadOptions.KEY);
			string storageLocation = _fileSystem.Path.Join(OFFLINE_CDN_LOCATION, videoUploadOptions.RemoteStorageLocation, fileName);
			var accessOptions = new OfflineRemoteStorageOptions(storageLocation);
			var uploadOptions = new OfflineRemoteStorageOptions(storageLocation);
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
			string remoteSubcontentPath = _fileSystem.Path.Join(OFFLINE_CDN_LOCATION, videoUploadOptions.RemoteStorageLocation, videoFileNameWithoutExtension);
			var ensureLocationResult = await _videoRemoteStorage.EnsureLocation(remoteSubcontentPath, RemoteLocationAccess.Public, cancellationToken);
			if(ensureLocationResult.IsError)
			{
				_logger.LogError("Failed to ensure video subcontent location {fromPath}, {videoFileName}.", fromPath, videoFileName);
				return ServiceResult<IEnumerable<Uri>>.Fail(ensureLocationResult.Code, ensureLocationResult.Error!);
			}
			var accessOptions = new OfflineRemoteStorageOptions(remoteSubcontentPath);
			var uploadOptions = new OfflineRemoteStorageOptions(remoteSubcontentPath);
			var uploadResult = await _videoRemoteStorage.Upload(fromPath, accessOptions, uploadOptions, cancellationToken);
			if (uploadResult.IsError)
			{
				_logger.LogError("Failed to upload video subcontent {fromPath}, {videoFileName}.", fromPath, videoFileName);
				return ServiceResult<IEnumerable<Uri>>.Fail(uploadResult.Code, uploadResult.Error!);
			}
			var uploadedFileNames = uploadResult.GetRequiredObject();
			var cdnUri = new Uri( videoUploadOptions.CdnUrl);
			var uploadedFilesUris = uploadedFileNames.Select(_ => new Uri(cdnUri, $"{videoUploadOptions.RemoteStorageLocation}/{videoFileNameWithoutExtension}/{Path.GetFileName(_)}")).ToArray();
			return ServiceResult<IEnumerable<Uri>>.Success(uploadedFilesUris);
		}
		public async Task<IServiceResult> DeleteAllVideoData(string videoFileName, CancellationToken cancellationToken = default)
		{
			VideoContentUploadOptions videoUploadOptions = _config.GetRequiredByKey<VideoContentUploadOptions>(VideoContentUploadOptions.KEY);
			var accessOptions = new OfflineRemoteStorageOptions(_fileSystem.Path.Join(videoUploadOptions.RemoteStorageLocation, videoFileName));
			var deleteVideoTask =  _videoRemoteStorage.Delete(accessOptions, cancellationToken);
			string fileNameWithoutExtension = _fileSystem.Path.GetFileNameWithoutExtension(videoFileName);
			string subcontentLocation = _fileSystem.Path.Join(videoUploadOptions.CdnUrl, videoUploadOptions.RemoteStorageLocation, fileNameWithoutExtension);
			var deleteSubcontentTask = _videoRemoteStorage.DeleteLocation(subcontentLocation);
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
