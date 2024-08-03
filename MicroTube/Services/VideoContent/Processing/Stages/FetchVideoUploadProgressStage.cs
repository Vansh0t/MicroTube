using MicroTube.Data.Access;
using MicroTube.Data.Models;

namespace MicroTube.Services.VideoContent.Processing.Stages
{
    public class FetchVideoUploadProgressStage : VideoProcessingStage
    {
        private readonly IVideoDataAccess _dataAccess;
        public FetchVideoUploadProgressStage(IVideoDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }
        protected override async Task<DefaultVideoProcessingContext> ExecuteInternal(DefaultVideoProcessingContext? context, CancellationToken cancellationToken)
        {
            ValidateContext(context);
            VideoUploadProgress uploadProgress = await GetUploadProgressForFileSourceVideo(context!.SourceVideoNormalizedName!);
            context.UploadProgress = uploadProgress;
            return context;
        }
        private void ValidateContext(DefaultVideoProcessingContext? context)
        {
            if (context == null)
            {
                throw new ArgumentNullException($"Context must not be null for stage {nameof(FetchVideoUploadProgressStage)}");
            }
            if (context.RemoteCache == null)
            {
                throw new ArgumentNullException($"{nameof(context.RemoteCache)} must not be null for stage {nameof(FetchVideoUploadProgressStage)}");
            }
            if (string.IsNullOrWhiteSpace(context.SourceVideoNormalizedName))
            {
                throw new ArgumentNullException($"{context.SourceVideoNormalizedName} must not be null for stage {nameof(FetchVideoUploadProgressStage)}");
            }
        }
        private async Task<VideoUploadProgress> GetUploadProgressForFileSourceVideo(string sourceName)
        {
            var uploadProgress = await _dataAccess.GetUploadProgressByFileName(sourceName);
            if (uploadProgress == null)
            {
                throw new BackgroundJobException($"Failed to get upload progress from db for video processing job. File: {sourceName}.");
            }
            return uploadProgress;
        }
    }
}
