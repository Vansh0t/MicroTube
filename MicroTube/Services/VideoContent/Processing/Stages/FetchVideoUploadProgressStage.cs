using Microsoft.EntityFrameworkCore;
using MicroTube.Data.Access;
using MicroTube.Data.Models;

namespace MicroTube.Services.VideoContent.Processing.Stages
{
    public class FetchVideoUploadProgressStage : VideoProcessingStage
    {
		private readonly MicroTubeDbContext _db;

		public FetchVideoUploadProgressStage(MicroTubeDbContext db)
		{
			_db = db;
		}

		protected override async Task<DefaultVideoProcessingContext> ExecuteInternal(DefaultVideoProcessingContext? context, CancellationToken cancellationToken)
        {
			if (context == null)
			{
				throw new ArgumentNullException($"Context must not be null for stage {nameof(FetchVideoUploadProgressStage)}");
			}
			if (context.RemoteCache == null)
			{
				throw new ArgumentNullException($"{nameof(context.RemoteCache)} must not be null for stage {nameof(FetchVideoUploadProgressStage)}");
			}
			VideoUploadProgress uploadProgress = await GetUploadProgressForFileSourceVideo(context.RemoteCache.VideoFileName);
            context.UploadProgress = uploadProgress;
            return context;
        }
        private async Task<VideoUploadProgress> GetUploadProgressForFileSourceVideo(string sourceName)
        {
			var uploadProgress = await _db.VideoUploadProgresses.FirstOrDefaultAsync(_ => _.SourceFileRemoteCacheFileName == sourceName);
            if (uploadProgress == null)
            {
                throw new BackgroundJobException($"Failed to get upload progress from db for video processing job. File: {sourceName}.");
            }
            return uploadProgress;
        }
    }
}
