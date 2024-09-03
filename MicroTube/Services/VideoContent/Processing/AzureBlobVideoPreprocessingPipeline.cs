using Azure.Storage.Blobs.Models;
using Hangfire;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.MediaContentStorage;
using MicroTube.Services.Validation;
using System.IO.Abstractions;
namespace MicroTube.Services.VideoContent.Processing
{
	public class AzureBlobVideoPreprocessingPipeline : IVideoPreprocessingPipeline<VideoPreprocessingOptions, VideoUploadProgress>
	{
		private readonly ILogger<AzureBlobVideoPreprocessingPipeline> _logger;
		private readonly IConfiguration _config;
		private readonly IVideoContentRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> _remoteStorage;
		private readonly IVideoPreUploadValidator _preUploadValidator;
		private readonly IVideoNameGenerator _videoNameGenerator;
		private readonly MicroTubeDbContext _db;
		private readonly IFileSystem _fileSystem;

		public AzureBlobVideoPreprocessingPipeline(
			ILogger<AzureBlobVideoPreprocessingPipeline> logger,
			IConfiguration config,
			IVideoContentRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> remoteStorage,
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
			VideoProcessingOptions processingOptions = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
			var uploadProgress = new VideoUploadProgress
			{
				UploaderId = new Guid(data.UserId),
				RemoteCacheLocation = processingOptions.RemoteStorageCacheLocation,
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
			var remoteCacheUploadResult = await UploadToRemoteCache(stream,generatedFileName, processingOptions.RemoteStorageCacheLocation);
			if(remoteCacheUploadResult.IsError)
			{
				_logger.LogError("Setting upload progress {uploadProgressId} to fail due to remote cache upload fail.", progress.Id);
				progress.Status = VideoUploadStatus.Fail;
				progress.Message = "Internal Error. Please, try again later.";
				await _db.SaveChangesAsync();
				return ServiceResult<VideoUploadProgress>.FailInternal();
			}
			// TO DO: uncomment
			//_backgroundJobClient.Enqueue<IVideoProcessingPipeline>(processing => processing.Process(generatedFileName, processingOptions.RemoteStorageCacheLocation, default));
			return ServiceResult<VideoUploadProgress>.Success(progress);
			
		}
		public IServiceResult ValidateUploadData(VideoPreprocessingOptions data)
		{
			IServiceResult fileValidationResult = _preUploadValidator.ValidateFile(data.VideoFile);
			IServiceResult titleValidationResult = _preUploadValidator.ValidateTitle(data.VideoTitle);
			IServiceResult descriptionValidationResult = _preUploadValidator.ValidateTitle(data.VideoDescription);
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
		private async Task<IServiceResult<VideoUploadProgress>> CreateVideoUploadProgress(VideoUploadProgress progress)
		{
			try
			{
				_db.Add(progress);
				await _db.SaveChangesAsync();
				return ServiceResult<VideoUploadProgress>.Success(progress);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to create a video upload entry. Cancelling the processing.");
				return ServiceResult<VideoUploadProgress>.FailInternal();
			}
		}
		private async Task<IServiceResult> UploadToRemoteCache(Stream stream, string fileName, string cacheLocation)
		{
			var blobUploadOptions = new BlobUploadOptions
			{
				AccessTier = AccessTier.Cold
			};
			var remoteUploadAccessOptions = new AzureBlobAccessOptions(fileName, cacheLocation);
			try
			{
				var remoteSaveResult = await _remoteStorage.Upload(stream, remoteUploadAccessOptions, blobUploadOptions);
				return ServiceResult.Success();
			}
			catch(Exception e)
			{
				return e.ExceptionToErrorResult();
			}
		}
	}
}
