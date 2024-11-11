using Ardalis.GuardClauses;
using MicroTube.Data.Access;
using MicroTube.Data.Models.Videos;

namespace MicroTube.Services.VideoContent.Processing.Stages
{
    public class CreateVideoInDatabaseStage : VideoProcessingStage
    {
		private MicroTubeDbContext _db;

		public CreateVideoInDatabaseStage(MicroTubeDbContext db)
		{
			_db = db;
		}

		protected override async Task<DefaultVideoProcessingContext> ExecuteInternal(DefaultVideoProcessingContext? context, CancellationToken cancellationToken)
        {
			Guard.Against.Null(context);
			Guard.Against.Null(context.UploadProgress);
			Guard.Against.Null(context.UploadProgress.LengthSeconds);
			Guard.Against.Null(context.RemoteCache);
			Guard.Against.Null(context.Cdn);
			Guard.Against.Null(context.Cdn.VideoEndpoints);
			Guard.Against.Null(context.Cdn.ThumbnailEndpoints);
			var video = new Video
			{
				UploaderId = context.UploadProgress.UploaderId,
				Title = context.UploadProgress.Title,
				Description = context.UploadProgress.Description,
				Urls = string.Join(';', context.Cdn.VideoEndpoints.Select(_ => _.ToString())),
				ThumbnailUrls = string.Join(';', context.Cdn.ThumbnailEndpoints.Select(_ => _.ToString())),
				UploadTime = DateTime.UtcNow,
				LengthSeconds = context.UploadProgress.LengthSeconds.Value,
				VideoIndexing = new VideoSearchIndexing { LastIndexingTime = null, ReindexingRequired = true, SearchIndexId = null },
				VideoReactionsAggregation = new VideoReactionsAggregation { Dislikes= 0, Likes =0},
				VideoViewsAggregation = new VideoViewsAggregation { Views = 0}
			};
			_db.Add(video);
			await _db.SaveChangesAsync();
            context.CreatedVideo = video;
            return context;
        }

    }
}
