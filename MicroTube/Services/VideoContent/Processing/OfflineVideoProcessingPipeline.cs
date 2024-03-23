using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.MediaContentStorage;

namespace MicroTube.Services.VideoContent.Processing
{
    public class OfflineVideoProcessingPipeline : IVideoProcessingPipeline
	{
		private readonly IConfiguration _config;
		private readonly ILogger<AzureBlobVideoProcessingPipeline> _logger;
		private readonly IVideoDataAccess _videoDataAccess;
		private readonly IVideoContentLocalStorage _videoLocalStorage;
		private readonly IVideoContentRemoteStorage<OfflineRemoteStorageOptions, OfflineRemoteStorageOptions> _remoteStorage;
		private readonly ICdnMediaContentAccess _mediaCdnAccess;
		private readonly IVideoThumbnailsService _thumbnailService;

		public OfflineVideoProcessingPipeline(
			IConfiguration config,
			ILogger<AzureBlobVideoProcessingPipeline> logger,
			IVideoDataAccess videoDataAccess,
			IVideoContentLocalStorage videoLocalStorage,
			ICdnMediaContentAccess mediaCdnAccess,
			IVideoThumbnailsService thumbnailService,
			IVideoContentRemoteStorage<OfflineRemoteStorageOptions, OfflineRemoteStorageOptions> remoteStorage)
		{
			_config = config;
			_logger = logger;
			_videoDataAccess = videoDataAccess;
			_videoLocalStorage = videoLocalStorage;
			_mediaCdnAccess = mediaCdnAccess;
			_thumbnailService = thumbnailService;
			_remoteStorage = remoteStorage;
		}
		public async Task Process(string videoFileName, string videoFileLocation, CancellationToken cancellationToken = default)
		{
			VideoProcessingOptions processingOptions = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
			VideoUploadProgress? uploadProgress = null;
			string? videoPath = null;
			try
			{
				uploadProgress = await GetVideoUploadProgressForFilePath(videoFileName);
				cancellationToken.ThrowIfCancellationRequested();
				await _videoDataAccess.UpdateUploadProgress(uploadProgress.Id.ToString(), VideoUploadStatus.InProgress);
				cancellationToken.ThrowIfCancellationRequested();
				videoPath = await DownloadFromRemoteProcessingCache(videoFileName, videoFileLocation, processingOptions.AbsoluteLocalStoragePath, cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();
				(IEnumerable<string> thumbnailPaths,
				IEnumerable<string> snapshotPaths) = await MakeImagesSubcontent(videoPath, processingOptions.AbsoluteLocalStoragePath, cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();
				var cdnVideoUrl = await UploadVideoToCdn(videoPath, videoFileName, cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();
				string subcontentDirectory = Path.Join(processingOptions.AbsoluteLocalStoragePath, Path.GetFileNameWithoutExtension(videoFileName));
				IEnumerable<Uri> subcontentUrls = await UploadVideoSubcontentToCdn(subcontentDirectory, videoFileName, cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();
				(string snapshotUrls, string thumbnailUrls) = FormatUrls(snapshotPaths, thumbnailPaths, subcontentUrls);
				var createdVideo = await _videoDataAccess.CreateVideo(
					new Video
					{
						UploaderId = uploadProgress.UploaderId,
						Title = uploadProgress.Title,
						Description = uploadProgress.Description,
						Url = cdnVideoUrl.ToString(),
						ThumbnailUrls = thumbnailUrls,
						SnapshotUrls = snapshotUrls,
						UploadTime = DateTime.UtcNow
					}
					);
				await _videoDataAccess.UpdateUploadProgress(uploadProgress.Id.ToString(), VideoUploadStatus.Success);
			}
			catch(Exception e)
			{
				try
				{
					//TODO: set to InQueue if retries are available
					await RevertProcessingOperation(videoFileName);
				}
				catch { }
				throw;
			}
			finally
			{
				try
				{
					await CleanupProcessingOperation(videoFileName, uploadProgress, videoPath, processingOptions);
				}
				catch { }
			}
		}
		private async Task<VideoUploadProgress> GetVideoUploadProgressForFilePath(string fileName)
		{
			var uploadProgress = await _videoDataAccess.GetUploadProgressByFileName(fileName);
			if (uploadProgress == null)
			{
				throw new BackgroundJobException($"Failed to get upload progress from db for video processing job. File: {fileName}.");
			}
			return uploadProgress;
		}
		private async Task<string> DownloadFromRemoteProcessingCache(string fileName, string fileLocation, string saveToPath, CancellationToken cancellationToken)
		{
			var remoteAccessOptions = new OfflineRemoteStorageOptions(Path.Join(fileLocation, fileName));
			var remoteCacheDownloadResult = await _remoteStorage.Download(saveToPath, remoteAccessOptions, cancellationToken);
			if(remoteCacheDownloadResult.IsError)
			{
				throw new BackgroundJobException($"Failed to download from remote cache for processing. File: {fileName}, Location: {fileLocation}, SaveTo: {saveToPath}.");
			}
			return remoteCacheDownloadResult.GetRequiredObject();
		}
		private async Task<(IEnumerable<string> thumbnails, IEnumerable<string> snapshots)> MakeImagesSubcontent(string videoFilePath, string saveToPath, CancellationToken cancellationToken)
		{
			var makeSnapshotsTask = _thumbnailService.MakeSnapshots(videoFilePath, saveToPath, cancellationToken);
			var makeThumbnailsTask = _thumbnailService.MakeThumbnails(videoFilePath, saveToPath, cancellationToken);
			await Task.WhenAll(makeSnapshotsTask, makeThumbnailsTask);
			var snapshotsResult = makeSnapshotsTask.Result;
			var thumbnailsResult = makeThumbnailsTask.Result;
			if (snapshotsResult.IsError)
			{
				throw new BackgroundJobException($"Failed to create snapshots. File: {videoFilePath}, SaveTo: {saveToPath}.");
			}
			if (thumbnailsResult.IsError)
			{
				throw new BackgroundJobException($"Failed to create thumbnails. File: {videoFilePath}, SaveTo: {saveToPath}.");
			}
			return (thumbnailsResult.GetRequiredObject(), snapshotsResult.GetRequiredObject());
		}
		private async Task<Uri> UploadVideoToCdn(string videoFilePath, string videoFileName, CancellationToken cancellationToken)
		{
			using var fileStream = new FileStream(videoFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			var videoUploadResult = await _mediaCdnAccess.UploadVideo(fileStream, videoFileName, cancellationToken);
			if (videoUploadResult.IsError)
			{
				throw new BackgroundJobException($"Failed to upload video file to CDN. Path: {videoFilePath}.");
			}
			return videoUploadResult.GetRequiredObject();
		}
		private async Task<IEnumerable<Uri>> UploadVideoSubcontentToCdn(string subcontentDirectory, string videoFileName, CancellationToken cancellationToken)
		{
			var subcontentUploadResult = await _mediaCdnAccess.UploadVideoSubcontent(subcontentDirectory, videoFileName, cancellationToken);
			if (subcontentUploadResult.IsError)
			{
				throw new BackgroundJobException($"Failed to upload video subcontent to CDN. File: {videoFileName}, SubcontentLocation: {subcontentDirectory}.");
			}
			return subcontentUploadResult.GetRequiredObject();
		}
		//TODO: this could use more optimized and robust solution
		private (string mergedSnapshotUrls, string mergedThumbnailUrls) FormatUrls(
			IEnumerable<string> snapshotPaths,
			IEnumerable<string> thumbnailPaths,
			IEnumerable<Uri> subcontentUrls)
		{
			var snapshotFileNames = snapshotPaths.Select(_ => Path.GetFileName(_));
			var thumbnailFileNames = thumbnailPaths.Select(_ => Path.GetFileName(_));
			var snapshotUris = snapshotFileNames.Select(_ => subcontentUrls.First(__ => __.OriginalString.Contains(_)));
			var thumbnailUris = thumbnailFileNames.Select(_ => subcontentUrls.First(__ => __.OriginalString.Contains(_)));
			string mergedSnapshotUrls = string.Join(";", snapshotUris.Select(_ => _.ToString()));
			string mergedThumbnailUrls = string.Join(";", thumbnailUris.Select(_ => _.ToString()));
			return (mergedSnapshotUrls, mergedThumbnailUrls);
		}
		private async Task RevertProcessingOperation(string videoFileName)
		{
			await _mediaCdnAccess.DeleteAllVideoData(videoFileName);
		}
		private async Task CleanupProcessingOperation(
			string videoFileName,
			VideoUploadProgress? videoUploadProgress,
			string? videoPath,
			VideoProcessingOptions processingOptions)
		{
			if (videoUploadProgress != null)
				await _videoDataAccess.UpdateUploadProgress(videoUploadProgress.Id.ToString(), VideoUploadStatus.Fail);
			if (videoPath != null)
				_videoLocalStorage.TryDelete(videoPath);
			_videoLocalStorage.TryDelete(Path.Join(processingOptions.AbsoluteLocalStoragePath, Path.GetFileNameWithoutExtension(videoFileName)));
		}
	}
}
