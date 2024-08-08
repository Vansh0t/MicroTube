using MicroTube.Data.Access;
using MicroTube.Data.Models;
using System.Diagnostics;

namespace MicroTube.Services.VideoContent.Processing.Stages
{
    public class SetProgressFinishedStage : VideoProcessingStage
    {
        private readonly IVideoAnalyzer _analyzer;
        private readonly IVideoDataAccess _dataAccess;

        public SetProgressFinishedStage(IVideoAnalyzer analyzer, IVideoDataAccess dataAccess)
        {
            _analyzer = analyzer;
            _dataAccess = dataAccess;
        }

        protected override async Task<DefaultVideoProcessingContext> ExecuteInternal(DefaultVideoProcessingContext? context, CancellationToken cancellationToken)
        {
            ValidateContext(context);
            context!.UploadProgress!.Status = VideoUploadStatus.Success;
            var processingTime = context.Stopwatch!.Elapsed;
            if (context.UploadProgress.Message == null)
                context.UploadProgress.Message = $"Upload successfully completed. Time: {processingTime}.";
            await _dataAccess.UpdateUploadProgress(context.UploadProgress);
            return context;
        }
        private void ValidateContext(DefaultVideoProcessingContext? context)
        {
            if (context == null)
            {
                throw new ArgumentNullException($"Context must not be null for stage {nameof(SetProgressInProgressStage)}");
            }
            if (context.UploadProgress == null)
            {
                throw new ArgumentNullException($"{nameof(context.UploadProgress)} must not be null for stage {nameof(SetProgressInProgressStage)}");
            }
            if (context.Stopwatch == null)
            {
                throw new ArgumentNullException($"{nameof(context.UploadProgress)} must not be null for stage {nameof(SetProgressInProgressStage)}");
            }
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
