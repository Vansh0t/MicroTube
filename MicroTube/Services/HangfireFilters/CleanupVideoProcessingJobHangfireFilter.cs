using Azure.Storage.Blobs.Models;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using MicroTube.Constants;
using MicroTube.Data.Access;
using MicroTube.Data.Models.Videos;
using MicroTube.Services.ContentStorage;
using MicroTube.Services.VideoContent.Processing.Stages;

namespace MicroTube.Services.HangfireFilters
{
    public class CleanupVideoProcessingJobHangfireFilter : JobFilterAttribute, IApplyStateFilter
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CleanupVideoProcessingJobHangfireFilter> _logger;
        public CleanupVideoProcessingJobHangfireFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<CleanupVideoProcessingJobHangfireFilter>>();
        }
        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            try
            {
				if(context.BackgroundJob.Job.Queue != HangfireConstants.VIDEO_PROCESSING_QUEUE)
				{
					return;
				}
                var failedState = context.NewState as FailedState;
				if(failedState == null)
				{
					return;
				}
				_logger.LogInformation($"Starting job {context.BackgroundJob.Id} cleanup.");
				DefaultVideoProcessingContext? processingContext = context.BackgroundJob.Job.Args
					.FirstOrDefault(_ => _ is DefaultVideoProcessingContext)
					as DefaultVideoProcessingContext;
				if(processingContext == null)
				{
					throw new BackgroundJobException("Failed to run video processing cleanup due to missing processing context");
				}
				using var scope = _serviceProvider.CreateScope();
				using var db = scope.ServiceProvider.GetRequiredService<MicroTubeDbContext>();
				var remoteStorage = scope.ServiceProvider.GetRequiredService<IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions>>();
				if (failedState != null)
                {
					ClearRemoteCache(processingContext, db, remoteStorage);
					SetUploadProgressFail(processingContext, db);
                }
            }
            catch (Exception e)
            {
                // Unhandled exceptions can cause an endless loop.
                // Therefore, only try logging safely.
                try
                {
                    _logger.LogError(e, "Failed to cleanup video processing job.");
                }
                catch { }
            }
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {

        }
        private void ClearRemoteCache(DefaultVideoProcessingContext processingContext, MicroTubeDbContext db, IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> remoteStorage)
        {
            if (processingContext.RemoteCache != null && !string.IsNullOrWhiteSpace(processingContext.RemoteCache.VideoFileLocation))
            {
                remoteStorage.DeleteLocation(processingContext.RemoteCache.VideoFileLocation).Wait();
            }
        }
		private void SetUploadProgressFail(DefaultVideoProcessingContext processingContext, MicroTubeDbContext db)
		{
			if(processingContext.RemoteCache == null)
			{
				return;
			}
			var progress = db.VideoUploadProgresses.FirstOrDefault(_ => _.SourceFileRemoteCacheFileName == processingContext.RemoteCache.VideoFileName);
			if(progress == null)
			{
				return;
			}
			progress.Status = VideoUploadStatus.Fail;
			progress.Message = "Failed to process the video. All attempts failed";
			db.SaveChanges();
		}
    }
}
