using Ardalis.GuardClauses;
using MicroTube.Data.Access;
using MicroTube.Data.Models;

namespace MicroTube.Services.VideoContent.Processing.Stages
{
    public class SetProgressInProgressStage : VideoProcessingStage
    {
        private readonly IVideoAnalyzer _analyzer;
		private readonly MicroTubeDbContext _db;

		public SetProgressInProgressStage(IVideoAnalyzer analyzer, MicroTubeDbContext db)
		{
			_analyzer = analyzer;
			_db = db;
		}

		protected override async Task<DefaultVideoProcessingContext> ExecuteInternal(DefaultVideoProcessingContext? context, CancellationToken cancellationToken)
        {
			Guard.Against.Null(context);
			Guard.Against.Null(context.UploadProgress);
			Guard.Against.Null(context.LocalCache);
			_db.Update(context.UploadProgress);
			string localCacheSourcePath = context.LocalCache.SourcePath;
            context.UploadProgress = await UpdateProgressFromAnalyzeResult(localCacheSourcePath, context.UploadProgress, cancellationToken);
            context.UploadProgress.Status = VideoUploadStatus.InProgress;
            EnsureUploadProgressLengthIsSet(context.UploadProgress);
			await _db.SaveChangesAsync();
            return context;
        }
        private async Task<VideoUploadProgress> UpdateProgressFromAnalyzeResult(string localCacheVideoSourcePath, VideoUploadProgress uploadProgress, CancellationToken cancellationToken)
        {
            var videoMetaData = await _analyzer.Analyze(localCacheVideoSourcePath, cancellationToken);
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
    }
}
