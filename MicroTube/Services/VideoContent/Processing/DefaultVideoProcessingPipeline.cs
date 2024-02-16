using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.MediaContentStorage;

namespace MicroTube.Services.VideoContent.Processing
{
    public class DefaultVideoProcessingPipeline : IVideoProcessingPipeline
	{
		private readonly IConfiguration _config;
		private readonly ILogger<DefaultVideoProcessingPipeline> _logger;
		private readonly IVideoDataAccess _videoDataAccess;
		private readonly IVideoContentLocalStorage _videoLocalStorage;
		private readonly ICdnMediaContentAccess _mediaCdnAccess;

		public DefaultVideoProcessingPipeline(
			IConfiguration config,
			ILogger<DefaultVideoProcessingPipeline> logger,
			IVideoDataAccess videoDataAccess,
			IVideoContentLocalStorage videoLocalStorage,
			ICdnMediaContentAccess mediaCdnAccess)
		{
			_config = config;
			_logger = logger;
			_videoDataAccess = videoDataAccess;
			_videoLocalStorage = videoLocalStorage;
			_mediaCdnAccess = mediaCdnAccess;
		}
		public async Task<IServiceResult> Process(string videoFilePath, CancellationToken cancellationToken = default)
		{
			VideoContentUploadOptions options = _config
				.GetRequiredSection(VideoContentUploadOptions.KEY)
				.GetRequired<VideoContentUploadOptions>();
			VideoUploadProgress? uploadProgress = null;
			try
			{
				uploadProgress = await GetVideoUploadProgressForFilePath(videoFilePath);
				cancellationToken.ThrowIfCancellationRequested();
				if(uploadProgress == null)
				{
					_logger.LogError("Unable to get or create upload progress entry for {filePath}", videoFilePath);
					return ServiceResult.FailInternal();
				}
				await _videoDataAccess.UpdateUploadProgress(uploadProgress.Id.ToString(), VideoUploadStatus.InProgress);
				cancellationToken.ThrowIfCancellationRequested();
				using var fileStream = new FileStream(videoFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, options.RemoteStorageUploadBufferSizeBytes);
				await _mediaCdnAccess.UploadVideo(fileStream, Path.GetFileName(videoFilePath), cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();
				await _videoDataAccess.UpdateUploadProgress(uploadProgress.Id.ToString(), VideoUploadStatus.Success);
				fileStream.Dispose();
				_videoLocalStorage.TryDelete(videoFilePath);
				return ServiceResult.Success();
			}
			catch(TaskCanceledException)
			{
				_logger.LogWarning("Video processing was cancelled for {videoPath}", videoFilePath);
				if (uploadProgress != null)
					await _videoDataAccess.UpdateUploadProgress(uploadProgress.Id.ToString(), VideoUploadStatus.Fail);
				return ServiceResult.FailInternal();
			}
			catch(Exception e)
			{
				_logger.LogWarning(e, "Video processing for {videoPath} failed due to unhandled exception", videoFilePath);
				if (uploadProgress != null)
					await _videoDataAccess.UpdateUploadProgress(uploadProgress.Id.ToString(), VideoUploadStatus.Fail);
				return ServiceResult.FailInternal();
			}
		}
		public void Test(string text)
		{
			_logger.LogInformation(text);
		}
		private async Task<VideoUploadProgress?> GetVideoUploadProgressForFilePath(string videoFilePath)
		{
			var uploadProgress = await _videoDataAccess.GetUploadProgressByLocalFullPath(videoFilePath);
			if (uploadProgress == null)
			{
				_logger.LogError("Failed to fetch upload progress for video file at path {videoFilePath}. Creating progress entry", videoFilePath);
			}
			return uploadProgress;
		}
	}
}
