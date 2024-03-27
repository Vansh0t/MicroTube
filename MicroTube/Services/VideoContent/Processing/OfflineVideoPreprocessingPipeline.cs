using Azure.Storage.Blobs.Models;
using Hangfire;
using MicroTube.Data.Access;
using MicroTube.Data.Access.SQLServer;
using MicroTube.Data.Models;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.MediaContentStorage;
using MicroTube.Services.Validation;
namespace MicroTube.Services.VideoContent.Processing
{
	public class OfflineVideoPreprocessingPipeline : IVideoPreprocessingPipeline<VideoPreprocessingOptions, VideoUploadProgress>
	{
		private readonly ILogger<OfflineVideoPreprocessingPipeline> _logger;
		private readonly IConfiguration _config;
		private readonly IVideoContentRemoteStorage<OfflineRemoteStorageOptions, OfflineRemoteStorageOptions> _remoteStorage;
		private readonly IVideoPreUploadValidator _preUploadValidator;
		private readonly IVideoNameGenerator _videoNameGenerator;
		private readonly IVideoDataAccess _videoDataAccess;
		private readonly IBackgroundJobClient _backgroundJobClient;

		public OfflineVideoPreprocessingPipeline(
			ILogger<OfflineVideoPreprocessingPipeline> logger,
			IConfiguration config,
			IVideoContentRemoteStorage<OfflineRemoteStorageOptions, OfflineRemoteStorageOptions> remoteStorage,
			IVideoPreUploadValidator preUploadValidator,
			IVideoNameGenerator videoNameGenerator,
			IVideoDataAccess videoDataAccess,
			IBackgroundJobClient backgroundJobClient)
		{
			_logger = logger;
			_config = config;
			_remoteStorage = remoteStorage;
			_preUploadValidator = preUploadValidator;
			_videoNameGenerator = videoNameGenerator;
			_videoDataAccess = videoDataAccess;
			_backgroundJobClient = backgroundJobClient;
		}

		public async Task<IServiceResult<VideoUploadProgress>> PreprocessVideo(VideoPreprocessingOptions data)
		{
			var validationResult = ValidateUploadData(data);
			if (validationResult.IsError)
				return ServiceResult<VideoUploadProgress>.Fail(validationResult.Code, validationResult.Error!);
			using var stream = data.VideoFile.OpenReadStream();
			string generatedFileName = _videoNameGenerator.GenerateVideoName() + Path.GetExtension(data.VideoFile.FileName);
			VideoProcessingOptions processingOptions = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
			var uploadProgressCreationOptions = new VideoUploadProgressCreationOptions(data.UserId,
																			  processingOptions.RemoteStorageCacheLocation,
																			  generatedFileName,
																			  data.VideoTitle,
																			  DateTime.UtcNow,
																			  data.VideoDescription);
			var progressCreationResult = await CreateVideoUploadProgress(uploadProgressCreationOptions);
			if(progressCreationResult.IsError)
				return ServiceResult<VideoUploadProgress>.Fail(progressCreationResult.Code, progressCreationResult.Error!);
			var progress = progressCreationResult.GetRequiredObject();
			//TO DO: add actual file validation
			var remoteCacheUploadResult = await UploadToRemoteCache(stream,generatedFileName, processingOptions.RemoteStorageCacheLocation);
			if(remoteCacheUploadResult.IsError)
			{
				_logger.LogError("Setting upload progress {uploadProgressId} to fail due to remote cache upload fail.", progress.Id);
				progress.Message = "Internal Error. Please, try again later.";
				progress.Status = VideoUploadStatus.Fail;
				await _videoDataAccess.UpdateUploadProgress(progress);
				return ServiceResult<VideoUploadProgress>.FailInternal();
			}
			_backgroundJobClient.Enqueue<IVideoProcessingPipeline>(processing => processing.Process(generatedFileName, processingOptions.RemoteStorageCacheLocation, default));
			return ServiceResult<VideoUploadProgress>.Success(progress);
			
		}
		public IServiceResult ValidateUploadData(VideoPreprocessingOptions data)
		{
			_logger.LogInformation(data.VideoTitle);
			IServiceResult fileValidationResult = _preUploadValidator.ValidateFile(data.VideoFile);
			IServiceResult titleValidationResult = _preUploadValidator.ValidateTitle(data.VideoTitle);
			IServiceResult descriptionValidationResult = _preUploadValidator.ValidateDescription(data.VideoDescription);
			string joinedError = "";
			if (fileValidationResult.IsError)
				joinedError += fileValidationResult.Error;
			if (titleValidationResult.IsError)
				joinedError += ", " + titleValidationResult.Error;
			if (descriptionValidationResult.IsError)
				joinedError += ", " + descriptionValidationResult.Error;
			if (!string.IsNullOrWhiteSpace(joinedError))
				return ServiceResult.Fail(400, joinedError);
			return ServiceResult.Success();
		}
		private async Task<IServiceResult<VideoUploadProgress>> CreateVideoUploadProgress(VideoUploadProgressCreationOptions options)
		{
			try
			{
				var uploadProgressEntry = await _videoDataAccess.CreateUploadProgress(options);
				if (uploadProgressEntry == null)
				{
					throw new DataAccessException("Upload progress creation returned null result.");
				}
				return ServiceResult<VideoUploadProgress>.Success(uploadProgressEntry);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to create a video upload entry. Cancelling the processing.");
				return ServiceResult<VideoUploadProgress>.FailInternal();
			}
		}
		private async Task<IServiceResult> UploadToRemoteCache(Stream stream, string fileName, string cacheLocation)
		{
			var uploadOptions = new OfflineRemoteStorageOptions(Path.Join(cacheLocation, fileName));
			var accessOptions = new OfflineRemoteStorageOptions(Path.Join(cacheLocation, fileName));
			var remoteSaveResult = await _remoteStorage.Upload(stream, accessOptions, uploadOptions);
			if (remoteSaveResult.IsError)
				return ServiceResult.Fail(remoteSaveResult.Code, remoteSaveResult.Error!);
			return ServiceResult.Success();
		}
	}
}
