using Azure.Storage.Blobs.Models;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.Base;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.MediaContentStorage;
using MicroTube.Services.VideoContent.Processing.Stages;
using System.Diagnostics;
using System.IO.Abstractions;

namespace MicroTube.Services.VideoContent.Processing
{
    public class AzureBlobVideoProcessingPipeline : IVideoProcessingPipeline
	{
		private readonly IConfiguration _config;
		private readonly ILogger<AzureBlobVideoProcessingPipeline> _logger;
		private readonly IVideoContentRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> _remoteStorage;
		private readonly ICdnMediaContentAccess _mediaCdnAccess;
		private readonly IVideoThumbnailsService _thumbnailService;
		private readonly IVideoAnalyzer _videoAnalyzer;
		private readonly MicroTubeDbContext _db;
		private readonly IFileSystem _fileSystem;

		public PipelineState State => throw new NotImplementedException();

		public AzureBlobVideoProcessingPipeline(
			IConfiguration config,
			ILogger<AzureBlobVideoProcessingPipeline> logger,
			ICdnMediaContentAccess mediaCdnAccess,
			IVideoThumbnailsService thumbnailService,
			IVideoContentRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> remoteStorage,
			IVideoAnalyzer videoAnalyzer,
			MicroTubeDbContext db,
			IFileSystem fileSystem)
		{
			_config = config;
			_logger = logger;
			_mediaCdnAccess = mediaCdnAccess;
			_thumbnailService = thumbnailService;
			_remoteStorage = remoteStorage;
			_videoAnalyzer = videoAnalyzer;
			_db = db;
			_fileSystem = fileSystem;
		}
		public async Task Process(string videoFileName, string videoFileLocation, CancellationToken cancellationToken = default)
		{
			//VideoProcessingOptions processingOptions = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
			//VideoUploadProgress? uploadProgress = null;
			//string? videoPath = null;
			//Stopwatch stopwatch = Stopwatch.StartNew();
			//try
			//{
			//	uploadProgress = await GetVideoUploadProgressForFilePath(videoFileName);
			//	cancellationToken.ThrowIfCancellationRequested();
			//	videoPath = await DownloadFromRemoteProcessingCache(videoFileName, videoFileLocation, processingOptions.AbsoluteLocalStoragePath, cancellationToken);
			//	cancellationToken.ThrowIfCancellationRequested();
			//	uploadProgress = await UpdateProgressFromAnalyzeResult(videoPath, uploadProgress, cancellationToken);
			//	uploadProgress.Status = VideoUploadStatus.InProgress;
			//	EnsureUploadProgressLengthIsSet(uploadProgress);
			//	await _videoDataAccess.UpdateUploadProgress(uploadProgress);
			//	cancellationToken.ThrowIfCancellationRequested();
			//	(IEnumerable<string> thumbnailPaths,
			//	IEnumerable<string> snapshotPaths) = await MakeImagesSubcontent(videoPath, processingOptions.AbsoluteLocalStoragePath, cancellationToken);
			//	cancellationToken.ThrowIfCancellationRequested();
			//	var cdnVideoUrl = await UploadVideoToCdn(videoPath, videoFileName, cancellationToken);
			//	cancellationToken.ThrowIfCancellationRequested();
			//	string subcontentDirectory = Path.Join(processingOptions.AbsoluteLocalStoragePath, Path.GetFileNameWithoutExtension(videoFileName));
			//	IEnumerable<Uri> subcontentUrls = await UploadVideoSubcontentToCdn(subcontentDirectory, videoFileName, cancellationToken);
			//	cancellationToken.ThrowIfCancellationRequested();
			//	(string snapshotUrls, string thumbnailUrls) = FormatUrls(snapshotPaths, thumbnailPaths, subcontentUrls);
			//	EnsureUploadProgressLengthIsSet(uploadProgress);
			//	var createdVideo = await _videoDataAccess.CreateVideo(
			//		new Video
			//		{
			//			UploaderId = uploadProgress.UploaderId,
			//			Title = uploadProgress.Title,
			//			Description = uploadProgress.Description,
			//			Urls = cdnVideoUrl.ToString(),
			//			ThumbnailUrls = thumbnailUrls,
			//			SnapshotUrls = snapshotUrls,
			//			UploadTime = DateTime.UtcNow,
			//			LengthSeconds = uploadProgress.LengthSeconds!.Value
			//		}
			//		);
			//	var processingTime = stopwatch.Elapsed;
			//	uploadProgress.Status = VideoUploadStatus.Success;
			//	if (uploadProgress.Message == null)
			//		uploadProgress.Message = $"Upload successfully completed. Time: {processingTime}.";
			//	await _videoDataAccess.UpdateUploadProgress(uploadProgress);
			//}
			//catch(Exception e)
			//{
			//	try
			//	{
			//		//TODO: set to InQueue if retries are available
			//		await RevertProcessingOperation(videoFileName);
			//		if (uploadProgress != null)
			//		{
			//			uploadProgress.Status = VideoUploadStatus.Fail;
			//			if (uploadProgress.Message == null)
			//				uploadProgress.Message = "Unknown error";
			//			await _videoDataAccess.UpdateUploadProgress(uploadProgress);
			//		}
			//	}
			//	catch { }
			//	throw;
			//}
			//finally
			//{
			//	try
			//	{
			//		CleanupProcessingOperation(videoFileName, videoPath, processingOptions);
			//	}
			//	catch { }
			//}
		}
		private async Task<VideoUploadProgress> GetVideoUploadProgressForFilePath(string fileName)
		{
			//var uploadProgress = await _videoDataAccess.GetUploadProgressByFileName(fileName);
			//if (uploadProgress == null)
			//{
			//	throw new BackgroundJobException($"Failed to get upload progress from db for video processing job. File: {fileName}.");
			//}
			//return uploadProgress;
			return null;
		}
		private async Task<string> DownloadFromRemoteProcessingCache(string fileName, string fileLocation, string saveToPath, CancellationToken cancellationToken)
		{
			var remoteAccessOptions = new AzureBlobAccessOptions(fileName, fileLocation);
			return await _remoteStorage.Download(saveToPath, remoteAccessOptions, cancellationToken);
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
				throw new BackgroundJobException($"Failed to create snapshots. File: {videoFilePath}, SaveTo: {saveToPath}. {snapshotsResult.Error}");
			}
			if (thumbnailsResult.IsError)
			{
				throw new BackgroundJobException($"Failed to create thumbnails. File: {videoFilePath}, SaveTo: {saveToPath}. {thumbnailsResult.Error}");
			}
			return (thumbnailsResult.GetRequiredObject(), snapshotsResult.GetRequiredObject());
		}
		private async Task<Uri> UploadVideoToCdn(string videoFilePath, string videoFileName, CancellationToken cancellationToken)
		{
			using var fileStream = new FileStream(videoFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			var videoUploadResult = await _mediaCdnAccess.UploadVideo(fileStream, videoFileName, "", cancellationToken);
			if (videoUploadResult.IsError)
			{
				throw new BackgroundJobException($"Failed to upload video file to CDN. Path: {videoFilePath}. {videoUploadResult.Error}");
			}
			return videoUploadResult.GetRequiredObject();
		}
		private async Task<IEnumerable<Uri>> UploadVideoSubcontentToCdn(string subcontentDirectory, string videoFileName, CancellationToken cancellationToken)
		{
			var subcontentUploadResult = await _mediaCdnAccess.UploadVideoSubcontent(subcontentDirectory, videoFileName, cancellationToken);
			if (subcontentUploadResult.IsError)
			{
				throw new BackgroundJobException($"Failed to upload video subcontent to CDN. File: {videoFileName}, SubcontentLocation: {subcontentDirectory}. {subcontentUploadResult.Error}");
			}
			return subcontentUploadResult.GetRequiredObject();
		}
		//TODO: this could use more optimized and robust solution
		private (string mergedSnapshotUrls, string mergedThumbnailUrls) FormatUrls(
			IEnumerable<string> snapshotPaths,
			IEnumerable<string> thumbnailPaths,
			IEnumerable<Uri> subcontentUrls)
		{
			var snapshotFileNames = snapshotPaths.Select(_ => _fileSystem.Path.GetFileName(_));
			var thumbnailFileNames = thumbnailPaths.Select(_ => _fileSystem.Path.GetFileName(_));
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
		private void CleanupProcessingOperation(
			string videoFileName,
			string? videoPath,
			VideoProcessingOptions processingOptions)
		{
			//if (videoPath != null)
			//	_videoLocalStorage.TryDelete(videoPath);
			//_videoLocalStorage.TryDelete(Path.Join(processingOptions.AbsoluteLocalStoragePath, Path.GetFileNameWithoutExtension(videoFileName)));
		}
		private async Task<VideoUploadProgress> UpdateProgressFromAnalyzeResult(string videoPath, VideoUploadProgress uploadProgress, CancellationToken cancellationToken)
		{
			var videoMetaData = await _videoAnalyzer.Analyze(videoPath, cancellationToken);
			uploadProgress.LengthSeconds = videoMetaData.LengthSeconds;
			uploadProgress.Fps = (int)videoMetaData.Fps;
			uploadProgress.Format = videoMetaData.Format;
			uploadProgress.FrameSize = videoMetaData.FrameSize;
			return uploadProgress;
		}
		private void EnsureUploadProgressLengthIsSet(VideoUploadProgress uploadProgress)
		{
			if (uploadProgress.LengthSeconds == null)
			{
				uploadProgress.Message = "Failed to read video duration";
				throw new BackgroundJobException("Failed to read video duration for upload progress " + uploadProgress.Id);
			}
		}

		public void AddStage(IPipelineStage<DefaultVideoProcessingContext> stage)
		{
			throw new NotImplementedException();
		}

		public void InsertStage(IPipelineStage<DefaultVideoProcessingContext> stage, int index)
		{
			throw new NotImplementedException();
		}

		public void RemoveStage(IPipelineStage<DefaultVideoProcessingContext> stage)
		{
			throw new NotImplementedException();
		}

		public void RemoveStageAt(int index)
		{
			throw new NotImplementedException();
		}

		public Task<DefaultVideoProcessingContext> Execute(DefaultVideoProcessingContext? context, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
	}
}
