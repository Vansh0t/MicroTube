using MicroTube.Data.Access;
using MicroTube.Data.Models;

namespace MicroTube.Services.VideoContent.Processing.Stages.Offline
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
			ValidateContext(context);
			var createdVideo = await _dataAccess.CreateVideo(
				new Video
				{
					UploaderId = context!.UploadProgress!.UploaderId,
					Title = context.UploadProgress.Title,
					Description = context.UploadProgress.Description,
					Url = context.Cdn!.VideoEndpoint!.ToString(),
					ThumbnailUrls = string.Join(';', context.Cdn.ThumbnailEndpoints!.Select(_=>_.ToString())),
					SnapshotUrls = "",
					UploadTime = DateTime.UtcNow,
					LengthSeconds = context.UploadProgress.LengthSeconds!.Value
				}
				);
			context.CreatedVideo = createdVideo;
			return context;
		}
		private void ValidateContext(DefaultVideoProcessingContext? context)
		{
			if (context == null)
			{
				throw new ArgumentNullException($"Context must not be null for stage {nameof(CreateVideoInDatabaseStage)}");
			}
			if (context.UploadProgress == null)
			{
				throw new ArgumentNullException($"{nameof(context.UploadProgress)} must not be null for stage {nameof(CreateVideoInDatabaseStage)}");
			}
			if (context.Cdn == null)
			{
				throw new ArgumentNullException($"{nameof(context.Cdn)} must not be null for stage {nameof(CreateVideoInDatabaseStage)}");
			}
			if (context.Cdn.VideoEndpoint == null)
			{
				throw new ArgumentNullException($"{nameof(context.Cdn.VideoEndpoint)} must not be null for stage {nameof(CreateVideoInDatabaseStage)}");
			}
			if (context.Cdn.ThumbnailEndpoints == null)
			{
				throw new ArgumentNullException($"{nameof(context.Cdn.ThumbnailEndpoints)} must not be null for stage {nameof(CreateVideoInDatabaseStage)}");
			}
		}

	}
}
