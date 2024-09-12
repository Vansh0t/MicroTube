using Azure.Storage.Blobs.Models;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.Base;
using MicroTube.Services.ContentStorage;
using MicroTube.Services.VideoContent.Processing.Stages;
using System.Diagnostics;
using System.IO.Abstractions;

namespace MicroTube.Services.VideoContent.Processing
{
	public class DefaultVideoProcessingPipeline : IsolatedExecutionPipeline<DefaultVideoProcessingContext>, IVideoProcessingPipeline
	{
		private readonly ILogger<DefaultVideoProcessingPipeline> _logger;
		private readonly MicroTubeDbContext _db;
		private readonly IFileSystem _fileSystem;
		private readonly IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> _remoteStorage;

		public DefaultVideoProcessingPipeline(
			IConfiguration config,
			ILogger<DefaultVideoProcessingPipeline> logger,
			IEnumerable<VideoProcessingStage> stages,
			ICdnMediaContentAccess mediaCdnAccess,
			MicroTubeDbContext db,
			IFileSystem fileSystem,
			IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> remoteStorage)
		{
			_logger = logger;
			foreach (var stage in stages)
			{
				AddStage(stage);
			}
			_db = db;
			_fileSystem = fileSystem;
			_remoteStorage = remoteStorage;
		}
		public async override Task<DefaultVideoProcessingContext> Execute(DefaultVideoProcessingContext? context, CancellationToken cancellationToken = default)
		{
			ThrowIfExecuting();
			if (context == null)
				throw new ArgumentNullException("InitialContext must be set for this Pipeline");
			context.Stopwatch = Stopwatch.StartNew();
			State = PipelineState.Executing;
			try
			{
				foreach(var stage in stages)
				{
					_logger.LogInformation("Executing video processing stage: " + stage.GetType());
					await stage.Execute(context, cancellationToken);
				}
				context.Stopwatch.Stop();
				return context;
			} 
			catch
			{
				try
				{
					State = PipelineState.Error;
					await HandleError(context);
				}
				catch { }
				throw;
			}
			finally
			{
				State = PipelineState.Finished;
				ClearLocalCache(context);
				if (context.Stopwatch != null)
					context.Stopwatch.Stop();
			}
		}
		private void ClearLocalCache(DefaultVideoProcessingContext context)
		{
			if (context.LocalCache == null || context.LocalCache.WorkingLocation == null)
				return;
			_fileSystem.TryDeleteFileOrDirectory(context.LocalCache.WorkingLocation);
		}
		private async Task HandleError(DefaultVideoProcessingContext context)
		{
			if (context.RemoteCache != null && !string.IsNullOrWhiteSpace(context.RemoteCache.VideoFileLocation))
				await _remoteStorage.DeleteLocation(context.RemoteCache.VideoFileLocation); //TO DO: make into account retry possibility
			if (context.UploadProgress != null)
			{
				_db.Update(context.UploadProgress);
				context.UploadProgress.Status = VideoUploadStatus.Fail;
				if (context.UploadProgress.Message == null)
					context.UploadProgress.Message = "Unknown error";
				await _db.SaveChangesAsync();
			}
		}
	}
}
