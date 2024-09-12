using Ardalis.GuardClauses;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using System.IO.Abstractions;

namespace MicroTube.Services.VideoContent.Preprocessing.Stages
{
	public class CreateUploadProgressStage : VideoPreprocessingStage
	{
		private readonly IFileSystem _fileSystem;
		private readonly MicroTubeDbContext _db;
		public CreateUploadProgressStage(IFileSystem fileSystem, MicroTubeDbContext db)
		{
			_fileSystem = fileSystem;
			_db = db;
		}

		protected override async Task<DefaultVideoPreprocessingContext> ExecuteInternal(DefaultVideoPreprocessingContext? context, CancellationToken cancellationToken)
		{
			Guard.Against.Null(context);
			Guard.Against.Null(context.PreprocessingData);
			Guard.Against.Null(context.RemoteCache);
			var uploadProgress = new VideoUploadProgress
			{
				UploaderId = new Guid(context.PreprocessingData.UserId),
				SourceFileRemoteCacheLocation = context.RemoteCache.VideoFileLocation,
				SourceFileRemoteCacheFileName = context.RemoteCache.VideoFileName,
				Title = context.PreprocessingData.VideoTitle,
				Timestamp = DateTime.UtcNow,
				Description = context.PreprocessingData.VideoDescription,
				Status = VideoUploadStatus.InQueue
			};
			_db.VideoUploadProgresses.Add(uploadProgress);
			await _db.SaveChangesAsync(cancellationToken);
			context.UploadProgress = uploadProgress;
			return context;
		}
	}
}
