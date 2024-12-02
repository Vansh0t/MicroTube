using Ardalis.GuardClauses;
using Azure.Storage.Blobs.Models;
using MicroTube.Data.Access;
using MicroTube.Data.Models.Videos;
using MicroTube.Services.ContentStorage;
using MicroTube.Services.Videos;
using System.IO.Abstractions;

namespace MicroTube.Services.VideoContent.Preprocessing.Stages
{
    public class CreateUploadProgressStage : VideoPreprocessingStage
	{
		private readonly IFileSystem _fileSystem;
		private readonly MicroTubeDbContext _db;
		private readonly IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> _videoRemoteStorage;
		private readonly IRemoteStorageVideoMetaHandler _metaReader;
		public CreateUploadProgressStage(
			IFileSystem fileSystem,
			MicroTubeDbContext db,
			IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> videoRemoteStorage,
			IRemoteStorageVideoMetaHandler metaReader)
		{
			_fileSystem = fileSystem;
			_db = db;
			_videoRemoteStorage = videoRemoteStorage;
			_metaReader = metaReader;
		}

		protected override async Task<DefaultVideoPreprocessingContext> ExecuteInternal(DefaultVideoPreprocessingContext? context, CancellationToken cancellationToken)
		{
			Guard.Against.Null(context);
			Guard.Against.Null(context.PreprocessingData);
			Guard.Against.Null(context.RemoteCache);
			var meta = await _videoRemoteStorage.GetLocationMetadata(context.RemoteCache.VideoFileLocation, cancellationToken);
			string? uploaderId = _metaReader.ReadUploaderId(meta);
			Guard.Against.NullOrWhiteSpace(uploaderId);
			if(context.PreprocessingData.UserId != uploaderId)
			{
				throw new ForbiddenException($"User {context.PreprocessingData.UserId} tried to start processing for not owned video source " +
					$"{context.PreprocessingData.GeneratedSourceFileName} of user {uploaderId}");
			}
			string? title = _metaReader.ReadTitle(meta);
			Guard.Against.NullOrWhiteSpace(title);
			string? description = _metaReader.ReadDescription(meta);
			var uploadProgress = new VideoUploadProgress
			{
				UploaderId = new Guid(context.PreprocessingData.UserId),
				SourceFileRemoteCacheLocation = context.RemoteCache.VideoFileLocation,
				SourceFileRemoteCacheFileName = context.RemoteCache.VideoFileName,
				Title = title,
				Timestamp = DateTime.UtcNow,
				Description = description,
				Status = VideoUploadStatus.InQueue
			};
			_db.VideoUploadProgresses.Add(uploadProgress);
			await _db.SaveChangesAsync(cancellationToken);
			context.UploadProgress = uploadProgress;
			return context;
		}
	}
}
