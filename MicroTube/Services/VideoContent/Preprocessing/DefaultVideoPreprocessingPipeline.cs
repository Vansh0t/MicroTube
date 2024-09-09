using Azure.Storage.Blobs.Models;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.Base;
using MicroTube.Services.ContentStorage;
using MicroTube.Services.VideoContent.Preprocessing.Stages;

namespace MicroTube.Services.VideoContent.Preprocessing
{
	public class DefaultVideoPreprocessingPipeline : IsolatedExecutionPipeline<DefaultVideoPreprocessingContext>, IVideoPreprocessingPipeline
	{
		private readonly ILogger<DefaultVideoPreprocessingPipeline> _logger;
		private readonly MicroTubeDbContext _db;
		private readonly IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> _remoteStorage;
		public DefaultVideoPreprocessingPipeline(
			IEnumerable<VideoPreprocessingStage> stages,
			ILogger<DefaultVideoPreprocessingPipeline> logger,
			MicroTubeDbContext db,
			IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> remoteStorage)
		{
			foreach (var stage in stages)
			{
				AddStage(stage);
			}
			_logger = logger;
			_db = db;
			_remoteStorage = remoteStorage;
		}
		public async override Task<DefaultVideoPreprocessingContext> Execute(DefaultVideoPreprocessingContext? context, CancellationToken cancellationToken = default)
		{
			ThrowIfExecuting();
			if (context == null)
				throw new ArgumentNullException("Context must be set for this Pipeline");
			State = PipelineState.Executing;
			try
			{
				foreach (var stage in stages)
				{
					_logger.LogInformation("Executing video preprocessing stage: " + stage.GetType());
					await stage.Execute(context, cancellationToken);
				}
				return context;
			}
			catch
			{
				try
				{
					State = PipelineState.Error;
					await ClearRemoteData(context);
					await SetUploadProgressError(context);
				}
				catch { }
				throw;
			}
		}
		private async Task ClearRemoteData (DefaultVideoPreprocessingContext context)
		{
			if (context.RemoteCache != null && !string.IsNullOrWhiteSpace(context.RemoteCache.VideoFileLocation))
				await _remoteStorage.DeleteLocation(context.RemoteCache.VideoFileLocation); //TO DO: make into account retry possibility
		}
		private async Task SetUploadProgressError(DefaultVideoPreprocessingContext context)
		{
			if (context.UploadProgress != null)
			{
				_db.Update(context.UploadProgress);
				context.UploadProgress.Status = VideoUploadStatus.Fail;
				if (context.UploadProgress.Message == null)
					context.UploadProgress.Message = "Preprocessing error. Please, try again later.";
				await _db.SaveChangesAsync();
			}
		}
	}
}
