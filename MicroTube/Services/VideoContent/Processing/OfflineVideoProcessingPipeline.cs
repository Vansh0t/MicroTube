using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.Base;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.MediaContentStorage;
using MicroTube.Services.VideoContent.Processing.Stages;
using System.Diagnostics;

namespace MicroTube.Services.VideoContent.Processing
{
	public class OfflineVideoProcessingPipeline : IVideoProcessingPipeline
	{
		public PipelineState State => _state;

		private PipelineState _state;
		private List<IPipelineStage<DefaultVideoProcessingContext>> _stages = new();
		private IConfiguration _config;
		private ILogger<OfflineVideoProcessingPipeline> _logger;
		private ICdnMediaContentAccess _mediaCdnAccess;
		private IVideoDataAccess _videoDataAccess;
		private IVideoContentLocalStorage _localStorage;

		public OfflineVideoProcessingPipeline(
			IConfiguration config,
			ILogger<OfflineVideoProcessingPipeline> logger,
			IEnumerable<VideoProcessingStage> stages,
			ICdnMediaContentAccess mediaCdnAccess,
			IVideoDataAccess videoDataAccess,
			IVideoContentLocalStorage localStorage)
		{
			_config = config;
			_logger = logger;
			foreach (var stage in stages)
			{
				AddStage(stage);
			}
			_mediaCdnAccess = mediaCdnAccess;
			_videoDataAccess = videoDataAccess;
			_localStorage = localStorage;
		}

		public void AddStage(IPipelineStage<DefaultVideoProcessingContext> stage)
		{
			ThrowIfExecuting();
			_stages.Add(stage);
		}


		public void InsertStage(IPipelineStage<DefaultVideoProcessingContext> stage, int index)
		{
			ThrowIfExecuting();
			_stages.Insert(index, stage);
		}

		public void RemoveStage(IPipelineStage<DefaultVideoProcessingContext> stage)
		{
			ThrowIfExecuting();
			_stages.Remove(stage);
		}

		public void RemoveStageAt(int index)
		{
			ThrowIfExecuting();
			_stages.RemoveAt(index);
		}
		public async Task<DefaultVideoProcessingContext> Execute(DefaultVideoProcessingContext? context, CancellationToken cancellationToken = default)
		{
			ThrowIfExecuting();
			if (context == null)
				throw new ArgumentNullException("InitialContext must be set for this Pipeline");
			context.Stopwatch = Stopwatch.StartNew();
			_state = PipelineState.Executing;
			try
			{
				foreach(var stage in _stages)
				{
					_logger.LogInformation("Executing video offline processing stage: " + stage.GetType());
					await stage.Execute(context, cancellationToken);
				}
				context.Stopwatch.Stop();
				return context;
			} 
			catch
			{
				try
				{
					if (!string.IsNullOrWhiteSpace(context.SourceVideoNormalizedName))
						await RevertProcessingOperation(context.SourceVideoNormalizedName);
					if (context.UploadProgress != null)
					{
						context.UploadProgress.Status = VideoUploadStatus.Fail;
						if (context.UploadProgress.Message == null)
							context.UploadProgress.Message = "Unknown error";
						await _videoDataAccess.UpdateUploadProgress(context.UploadProgress);
					}
				}
				catch { }
				throw;
			}
			finally
			{
				if(context.LocalCache != null)
				{
					string pathToVideo = Path.Join(context.LocalCache.VideoFileLocation, context.LocalCache.VideoFileName);
					if(pathToVideo.Length>5)
					{
						CleanupLocalCache(pathToVideo);
					}
				}
				if (context.Stopwatch != null)
					context.Stopwatch.Stop();
				
			}
		}
		private void ThrowIfExecuting()
		{
			if (_state == PipelineState.Executing)
				throw new BackgroundJobException("Operation is not allowed while the pipeline is executing");
		}
		private async Task RevertProcessingOperation(string videoFileName)
		{
			await _mediaCdnAccess.DeleteAllVideoData(videoFileName);
		}
		private void CleanupLocalCache(string videoPath)
		{
			VideoProcessingOptions options = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
		    _localStorage.TryDelete(videoPath);
			_localStorage.TryDelete(Path.Join(options.AbsoluteLocalStoragePath, Path.GetFileNameWithoutExtension(videoPath)));
		}
		//private readonly IConfiguration _config;
		//private readonly ILogger<AzureBlobVideoProcessingPipeline> _logger;
		//private readonly IVideoDataAccess _videoDataAccess;
		//private readonly IVideoContentLocalStorage _videoLocalStorage;
		//private readonly IVideoContentRemoteStorage<OfflineRemoteStorageOptions, OfflineRemoteStorageOptions> _remoteStorage;
		//private readonly ICdnMediaContentAccess _mediaCdnAccess;
		//private readonly IVideoThumbnailsService _thumbnailService;
		//private readonly IVideoAnalyzer _videoAnalyzer;
		//private readonly IVideoCompressionService _compressionService;
		//
		//public OfflineVideoProcessingPipeline(
		//	IConfiguration config,
		//	ILogger<AzureBlobVideoProcessingPipeline> logger,
		//	IVideoDataAccess videoDataAccess,
		//	IVideoContentLocalStorage videoLocalStorage,
		//	ICdnMediaContentAccess mediaCdnAccess,
		//	IVideoThumbnailsService thumbnailService,
		//	IVideoContentRemoteStorage<OfflineRemoteStorageOptions, OfflineRemoteStorageOptions> remoteStorage,
		//	IVideoAnalyzer videoAnalyzer,
		//	IVideoCompressionService compressionService)
		//{
		//	_config = config;
		//	_logger = logger;
		//	_videoDataAccess = videoDataAccess;
		//	_videoLocalStorage = videoLocalStorage;
		//	_mediaCdnAccess = mediaCdnAccess;
		//	_thumbnailService = thumbnailService;
		//	_remoteStorage = remoteStorage;
		//	_videoAnalyzer = videoAnalyzer;
		//	_compressionService = compressionService;
		//}
		//public async Task Process(string videoFileName, string videoFileLocation, CancellationToken cancellationToken = default)
		//{
		//	VideoProcessingOptions processingOptions = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
		//	VideoUploadProgress? uploadProgress = null;
		//	string? videoPath = null;
		//	Stopwatch stopwatch = Stopwatch.StartNew();
		//	try
		//	{
		//		1uploadProgress = await GetVideoUploadProgressForFilePath(videoFileName);
		//		cancellationToken.ThrowIfCancellationRequested();
		//		2videoPath = await DownloadFromRemoteProcessingCache(videoFileName, videoFileLocation, processingOptions.AbsoluteLocalStoragePath, cancellationToken);
		//		cancellationToken.ThrowIfCancellationRequested();
		//		uploadProgress = await UpdateProgressFromAnalyzeResult(videoPath, uploadProgress, cancellationToken);
		//		uploadProgress.Status = VideoUploadStatus.InProgress;
		//		EnsureUploadProgressLengthIsSet(uploadProgress);
		//		3await _videoDataAccess.UpdateUploadProgress(uploadProgress);
		//		cancellationToken.ThrowIfCancellationRequested();
		//		4IDictionary<int, string> qualityTiers = await MakeQualityTiers(videoPath, processingOptions.AbsoluteLocalStoragePath, cancellationToken);
		//		string sourceForThumbnails = GetSourceFileForThumbnails(processingOptions.ThumbnailsQualityTier, qualityTiers);
		//		5IEnumerable<string> thumbnailPaths = await MakeThumbnails(sourceForThumbnails, processingOptions.AbsoluteLocalStoragePath, cancellationToken);
		//		cancellationToken.ThrowIfCancellationRequested();
		//		string subcontentDirectory = Path.Join(processingOptions.AbsoluteLocalStoragePath, Path.GetFileNameWithoutExtension(videoFileName));
		//		IEnumerable<Uri> subcontentUrls = await UploadVideoContentToCdn(subcontentDirectory, videoFileName, cancellationToken);
		//		cancellationToken.ThrowIfCancellationRequested();
		//		
		//		string thumbnailUrls = FormatThumbnailUrls(thumbnailPaths, subcontentUrls);
		//		EnsureUploadProgressLengthIsSet(uploadProgress);
		//		var createdVideo = await _videoDataAccess.CreateVideo(
		//			new Video
		//			{
		//				UploaderId = uploadProgress.UploaderId,
		//				Title = uploadProgress.Title,
		//				Description = uploadProgress.Description,
		//				Url = cdnVideoUrl.ToString(),
		//				ThumbnailUrls = thumbnailUrls,
		//				SnapshotUrls = "",
		//				UploadTime = DateTime.UtcNow,
		//				LengthSeconds = uploadProgress.LengthSeconds!.Value
		//			}
		//			);
		//		var processingTime = stopwatch.Elapsed;
		//		uploadProgress.Status = VideoUploadStatus.Success;
		//		if (uploadProgress.Message == null)
		//			uploadProgress.Message = $"Upload successfully completed. Time: {processingTime}.";
		//		await _videoDataAccess.UpdateUploadProgress(uploadProgress);
		//	}
		//	catch (Exception e)
		//	{
		//		try
		//		{
		//			//TODO: set to InQueue if retries are available
		//			await RevertProcessingOperation(videoFileName);
		//			if (uploadProgress != null)
		//			{
		//				uploadProgress.Status = VideoUploadStatus.Fail;
		//				if (uploadProgress.Message == null)
		//					uploadProgress.Message = "Unknown error";
		//				await _videoDataAccess.UpdateUploadProgress(uploadProgress);
		//			}
		//		}
		//		catch { }
		//		throw;
		//	}
		//	finally
		//	{
		//		try
		//		{
		//			CleanupProcessingOperation(videoFileName, videoPath, processingOptions);
		//			stopwatch.Stop();
		//		}
		//		catch { }
		//	}
		//}
		//private async Task<VideoUploadProgress> GetVideoUploadProgressForFilePath(string fileName)
		//{
		//	var uploadProgress = await _videoDataAccess.GetUploadProgressByFileName(fileName);
		//	if (uploadProgress == null)
		//	{
		//		throw new BackgroundJobException($"Failed to get upload progress from db for video processing job. File: {fileName}.");
		//	}
		//	return uploadProgress;
		//}
		//private async Task<string> DownloadFromRemoteProcessingCache(string fileName, string fileLocation, string saveToPath, CancellationToken cancellationToken)
		//{
		//	var remoteAccessOptions = new OfflineRemoteStorageOptions(Path.Join(fileLocation, fileName));
		//	var remoteCacheDownloadResult = await _remoteStorage.Download(saveToPath, remoteAccessOptions, cancellationToken);
		//	if (remoteCacheDownloadResult.IsError)
		//	{
		//		throw new BackgroundJobException($"Failed to download from remote cache for processing. File: {fileName}, Location: {fileLocation}, SaveTo: {saveToPath}.");
		//	}
		//	return remoteCacheDownloadResult.GetRequiredObject();
		//}
		//private async Task<IEnumerable<string>> MakeThumbnails(string videoFilePath, string saveToPath, CancellationToken cancellationToken)
		//{
		//	//var makeSnapshotsTask = _thumbnailService.MakeSnapshots(videoFilePath, saveToPath, cancellationToken);
		//	var thumbnailsResult = await _thumbnailService.MakeThumbnails(videoFilePath, saveToPath, cancellationToken);
		//	//await Task.WhenAll(makeSnapshotsTask, makeThumbnailsTask);
		//	//var snapshotsResult = makeSnapshotsTask.Result;
		//	//if (snapshotsResult.IsError)
		//	//{
		//	//	throw new BackgroundJobException($"Failed to create snapshots. File: {videoFilePath}, SaveTo: {saveToPath}. {snapshotsResult.Error}");
		//	//}
		//	if (thumbnailsResult.IsError)
		//	{
		//		throw new BackgroundJobException($"Failed to create thumbnails. File: {videoFilePath}, SaveTo: {saveToPath}. {thumbnailsResult.Error}");
		//	}
		//	return (thumbnailsResult.GetRequiredObject());
		//}
		//private async Task<IEnumerable<Uri>> UploadQualityTiers(IEnumerable<string>)
		//private async Task<Uri> UploadVideoToCdn(string videoFilePath, string videoFileName, CancellationToken cancellationToken)
		//{
		//	using var fileStream = new FileStream(videoFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		//	var videoUploadResult = await _mediaCdnAccess.UploadVideo(fileStream, videoFileName, cancellationToken);
		//	if (videoUploadResult.IsError)
		//	{
		//		throw new BackgroundJobException($"Failed to upload video file to CDN. Path: {videoFilePath}. {videoUploadResult.Error}");
		//	}
		//	return videoUploadResult.GetRequiredObject();
		//}
		//private async Task<IEnumerable<Uri>> UploadVideoContentToCdn(string subcontentDirectory, string videoFileName, CancellationToken cancellationToken)
		//{
		//	var subcontentUploadResult = await _mediaCdnAccess.UploadVideoSubcontent(subcontentDirectory, videoFileName, cancellationToken);
		//	if (subcontentUploadResult.IsError)
		//	{
		//		throw new BackgroundJobException($"Failed to upload video subcontent to CDN. File: {videoFileName}, SubcontentLocation: {subcontentDirectory}. {subcontentUploadResult.Error}");
		//	}
		//	return subcontentUploadResult.GetRequiredObject();
		//}
		////TODO: this could use more optimized and robust solution
		//private string FormatThumbnailUrls(
		//	/*IEnumerable<string> snapshotPaths,*/
		//	IEnumerable<string> thumbnailPaths,
		//	IEnumerable<Uri> subcontentUrls)
		//{
		//	//var snapshotFileNames = snapshotPaths.Select(_ => Path.GetFileName(_));
		//	var thumbnailFileNames = thumbnailPaths.Select(_ => Path.GetFileName(_));
		//	//var snapshotUris = snapshotFileNames.Select(_ => subcontentUrls.First(__ => __.OriginalString.Contains(_)));
		//	var thumbnailUris = thumbnailFileNames.Select(_ => subcontentUrls.First(__ => __.OriginalString.Contains(_)));
		//	//string mergedSnapshotUrls = string.Join(";", snapshotUris.Select(_ => _.ToString()));
		//	string mergedThumbnailUrls = string.Join(";", thumbnailUris.Select(_ => _.ToString()));
		//	return mergedThumbnailUrls;
		//}


		//private async Task<VideoUploadProgress> UpdateProgressFromAnalyzeResult(string videoPath, VideoUploadProgress uploadProgress, CancellationToken cancellationToken)
		//{
		//	var videoMetaData = await _videoAnalyzer.Analyze(videoPath, cancellationToken);
		//	uploadProgress.LengthSeconds = videoMetaData.LengthSeconds;
		//	uploadProgress.Fps = (int)videoMetaData.Fps;
		//	uploadProgress.Format = videoMetaData.Format;
		//	uploadProgress.FrameSize = videoMetaData.FrameSize;
		//	return uploadProgress;
		//}
		//private void EnsureUploadProgressLengthIsSet(VideoUploadProgress uploadProgress)
		//{
		//	if (uploadProgress.LengthSeconds == null)
		//	{
		//		uploadProgress.Message = "Failed to read video duration";
		//		throw new BackgroundJobException("Failed to read video duration for upload progress " + uploadProgress.Id);
		//	}
		//}
		//private async Task<IDictionary<int, string>> MakeQualityTiers(string sourcePath, string outputPath, CancellationToken cancellationToken = default)
		//{
		//	VideoProcessingOptions options = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
		//	var result = await _compressionService.CompressToQualityTiers(options.QualityTiers, sourcePath, outputPath, cancellationToken);
		//	if (result.IsError)
		//	{
		//		throw new BackgroundJobException($"Failed to make quality tiers. Source: {sourcePath}. {result.Error}");
		//	}
		//	return result.GetRequiredObject();
		//}
		//private string GetSourceFileForThumbnails(int tier, IDictionary<int, string> qualityTiers)
		//{
		//	if (qualityTiers.TryGetValue(tier, out var path))
		//	{
		//		return path;
		//	}
		//	throw new BackgroundJobException($"Unable to find quality tier for thumbnails. Requested tier: {tier}");
		//}

	}
}
