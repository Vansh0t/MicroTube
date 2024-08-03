using MicroTube.Data.Access;
using MicroTube.Data.Models;

namespace MicroTube.Services.VideoContent.Processing.Stages
{
    public class SetProgressInProgressStage : VideoProcessingStage
    {
        private readonly IVideoAnalyzer _analyzer;
        private readonly IVideoDataAccess _dataAccess;

        public SetProgressInProgressStage(IVideoAnalyzer analyzer, IVideoDataAccess dataAccess)
        {
            _analyzer = analyzer;
            _dataAccess = dataAccess;
        }

        protected override async Task<DefaultVideoProcessingContext> ExecuteInternal(DefaultVideoProcessingContext? context, CancellationToken cancellationToken)
        {
            ValidateContext(context);
            string localCacheSourcePath = Path.Join(context!.LocalCache!.VideoFileLocation, context.LocalCache.VideoFileName);
            context!.UploadProgress = await UpdateProgressFromAnalyzeResult(localCacheSourcePath, context.UploadProgress!, cancellationToken);
            context.UploadProgress.Status = VideoUploadStatus.InProgress;
            EnsureUploadProgressLengthIsSet(context.UploadProgress);
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
            if (context.LocalCache == null)
            {
                throw new ArgumentNullException($"{nameof(context.RemoteCache)} must not be null for stage {nameof(SetProgressInProgressStage)}");
            }
            if (string.IsNullOrWhiteSpace(Path.Join(context.LocalCache.VideoFileLocation, context.LocalCache.VideoFileName)))
            {
                throw new ArgumentNullException($"Joined local cache source path must not be null for stage {nameof(SetProgressInProgressStage)}");
            }
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
