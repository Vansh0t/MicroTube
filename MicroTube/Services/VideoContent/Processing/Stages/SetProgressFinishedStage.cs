using MicroTube.Data.Access;
using MicroTube.Data.Models;

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
			_db.Update(context.UploadProgress);
			context.UploadProgress.Status = VideoUploadStatus.Success;
            var processingTime = context.Stopwatch!.Elapsed;
            if (context.UploadProgress.Message == null)
                context.UploadProgress.Message = $"Upload successfully completed. Time: {processingTime}.";
			await _db.SaveChangesAsync();
            return context;
        }
    }
}
