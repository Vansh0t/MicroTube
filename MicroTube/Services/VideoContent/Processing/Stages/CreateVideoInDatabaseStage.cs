using MicroTube.Data.Access;
using MicroTube.Data.Models;

namespace MicroTube.Services.VideoContent.Processing.Stages
{
    public class CreateVideoInDatabaseStage : VideoProcessingStage
    {
        private readonly IVideoDataAccess _dataAccess;

        public CreateVideoInDatabaseStage(IVideoDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        protected override async Task<DefaultVideoProcessingContext> ExecuteInternal(DefaultVideoProcessingContext? context, CancellationToken cancellationToken)
        {
			if (context == null)
			{
				throw new ArgumentNullException($"Context must not be null for stage {nameof(CreateVideoInDatabaseStage)}");
			}
			if (context.UploadProgress == null)
			{
				throw new ArgumentNullException($"{nameof(context.UploadProgress)} must not be null for stage {nameof(CreateVideoInDatabaseStage)}");
			}
			if (context.RemoteCache == null)
			{
				throw new ArgumentNullException($"{nameof(context.RemoteCache)} must not be null for stage {nameof(CreateVideoInDatabaseStage)}");
			}
			if (context.UploadProgress == null)
			{
				throw new ArgumentNullException($"{nameof(context.UploadProgress)} must not be null for stage {nameof(CreateVideoInDatabaseStage)}");
			}
			if (context.Cdn == null || context.Cdn.VideoEndpoints == null || context.Cdn.ThumbnailEndpoints == null)
			{
				throw new ArgumentNullException($"Context cdn and endpoints must be set for stage {nameof(CreateVideoInDatabaseStage)}");
			}
			var createdVideo = await _dataAccess.CreateVideo(
                new Video
                {
                    UploaderId = context.UploadProgress.UploaderId,
                    Title = context.UploadProgress.Title,
                    Description = context.UploadProgress.Description,
                    Urls = string.Join(';', context.Cdn.VideoEndpoints.Select(_ => _.ToString())),
                    ThumbnailUrls = string.Join(';', context.Cdn.ThumbnailEndpoints.Select(_ => _.ToString())),
                    SnapshotUrls = "",
                    UploadTime = DateTime.UtcNow,
                    LengthSeconds = context.UploadProgress.LengthSeconds.Value
                }
                );
            context.CreatedVideo = createdVideo;
            return context;
        }

    }
}
