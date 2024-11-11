using Ardalis.GuardClauses;
using MicroTube.Data.Access;
using MicroTube.Data.Models.Videos;

namespace MicroTube.Services.VideoContent.Processing.Stages
{
    public class SetProgressFinishedStage : VideoProcessingStage
    {
        private readonly IVideoAnalyzer _analyzer;
		private readonly MicroTubeDbContext _db;

		public SetProgressFinishedStage(IVideoAnalyzer analyzer, MicroTubeDbContext db)
		{
			_analyzer = analyzer;
			_db = db;
		}

		protected override async Task<DefaultVideoProcessingContext> ExecuteInternal(DefaultVideoProcessingContext? context, CancellationToken cancellationToken)
        {
			Guard.Against.Null(context);
			Guard.Against.Null(context.UploadProgress);
			_db.Update(context.UploadProgress);
			context.UploadProgress.Status = VideoUploadStatus.Success;
            var processingTime = context.Stopwatch != null? context.Stopwatch.Elapsed : TimeSpan.FromSeconds(-1);
            if (context.UploadProgress.Message == null)
                context.UploadProgress.Message = $"Upload successfully completed. Time: {processingTime}.";
			await _db.SaveChangesAsync();
            return context;
        }
    }
}
