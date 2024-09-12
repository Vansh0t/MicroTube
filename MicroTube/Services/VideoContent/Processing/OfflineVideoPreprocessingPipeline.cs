using Hangfire;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.MediaContentStorage;
using MicroTube.Services.Validation;
using MicroTube.Services.VideoContent.Processing.Stages;
using System.IO.Abstractions;

namespace MicroTube.Services.VideoContent.Processing
{
	public class OfflineVideoPreprocessingPipeline : IVideoPreprocessingPipeline<VideoPreprocessingOptions, VideoUploadProgress>
	{
		private readonly ILogger<OfflineVideoPreprocessingPipeline> _logger;
		private readonly IConfiguration _config;
		private readonly IVideoContentRemoteStorage<OfflineRemoteStorageOptions, OfflineRemoteStorageOptions> _remoteStorage;
		private readonly IVideoPreUploadValidator _preUploadValidator;
		private readonly IVideoNameGenerator _videoNameGenerator;
		private readonly IBackgroundJobClient _backgroundJobClient;
		private readonly MicroTubeDbContext _db;
		private readonly IFileSystem _fileSystem;

		public OfflineVideoPreprocessingPipeline(
			ILogger<OfflineVideoPreprocessingPipeline> logger,
			IConfiguration config,
			IVideoContentRemoteStorage<OfflineRemoteStorageOptions, OfflineRemoteStorageOptions> remoteStorage,
			IVideoPreUploadValidator preUploadValidator,
			IVideoNameGenerator videoNameGenerator,
			IBackgroundJobClient backgroundJobClient,
			MicroTubeDbContext db,
			IFileSystem fileSystem)
		{
			_logger = logger;
			_config = config;
			_remoteStorage = remoteStorage;
			_preUploadValidator = preUploadValidator;
			_videoNameGenerator = videoNameGenerator;
			_backgroundJobClient = backgroundJobClient;
			_db = db;
			_fileSystem = fileSystem;
		}

		public async Task<IServiceResult<VideoUploadProgress>> PreprocessVideo(VideoPreprocessingOptions data)
		{
			var validationResult = ValidateUploadData(data);
			if (validationResult.IsError)
				return ServiceResult<VideoUploadProgress>.Fail(validationResult.Code, validationResult.Error!);
			using var stream = data.VideoFile.OpenReadStream();
			string generatedFileName = _videoNameGenerator.GenerateVideoName() + _fileSystem.Path.GetExtension(data.VideoFile.FileName);
			VideoProcessingOptions options = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
			var uploadProgress = new VideoUploadProgress
			{
				UploaderId = new Guid(data.UserId),
				RemoteCacheLocation = options.RemoteStorageCacheLocation,
				RemoteCacheFileName = generatedFileName,
				Title = data.VideoTitle,
				Timestamp = DateTime.UtcNow,
				Description = data.VideoDescription,
				Status = VideoUploadStatus.InQueue
			};
			var progressCreationResult = await CreateVideoUploadProgress(uploadProgress);
			if(progressCreationResult.IsError)
				return ServiceResult<VideoUploadProgress>.Fail(progressCreationResult.Code, progressCreationResult.Error!);
			var progress = progressCreationResult.GetRequiredObject();
			//TO DO: add actual file validation
			var remoteCacheUploadResult = await UploadToRemoteCache(stream,generatedFileName, options.RemoteStorageCacheLocation);
			if(remoteCacheUploadResult.IsError)
			{
				_logger.LogError("Setting upload progress {uploadProgressId} to fail due to remote cache upload fail. {error}", progress.Id, remoteCacheUploadResult.Error);
				uploadProgress.Message = "Internal Error. Please, try again later.";
				uploadProgress.Status = VideoUploadStatus.Fail;
				await _db.SaveChangesAsync();
				return ServiceResult<VideoUploadProgress>.FailInternal();
			}
			_backgroundJobClient.Enqueue<IVideoProcessingPipeline>("video_processing",
				processing => processing.Execute(
					new DefaultVideoProcessingContext() 
					{ 
						SourceVideoNameWithoutExtension = _fileSystem.Path.GetFileNameWithoutExtension(generatedFileName), 
						RemoteCache = new VideoProcessingRemoteCache 
						{ 
							VideoFileName = generatedFileName, 
							VideoFileLocation = options.RemoteStorageCacheLocation
						}
				}, default));
			return ServiceResult<VideoUploadProgress>.Success(progress);
			
		}
		public IServiceResult ValidateUploadData(VideoPreprocessingOptions data)
		{
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
		private async Task<IServiceResult<VideoUploadProgress>> CreateVideoUploadProgress(VideoUploadProgress options)
		{
			try
			{
				_db.Add(options);
				await _db.SaveChangesAsync();
				return ServiceResult<VideoUploadProgress>.Success(options);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to create a video upload entry. Cancelling the processing.");
				return ServiceResult<VideoUploadProgress>.FailInternal();
			}
		}
		private async Task<IServiceResult> UploadToRemoteCache(Stream stream, string fileName, string cacheLocation)
		{
			var uploadOptions = new OfflineRemoteStorageOptions(_fileSystem.Path.Join(cacheLocation, fileName));
			var accessOptions = new OfflineRemoteStorageOptions(_fileSystem.Path.Join(cacheLocation, fileName));
			var remoteSaveResult = await _remoteStorage.Upload(stream, accessOptions, uploadOptions);
			if (remoteSaveResult.IsError)
				return ServiceResult.Fail(remoteSaveResult.Code, remoteSaveResult.Error!);
			return ServiceResult.Success();
		}
	}
}
