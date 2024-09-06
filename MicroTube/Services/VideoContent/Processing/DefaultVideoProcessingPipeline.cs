using Azure.Storage.Blobs.Models;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.Base;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.MediaContentStorage;
using MicroTube.Services.VideoContent.Processing.Stages;
using System.Diagnostics;
using System.IO.Abstractions;

namespace MicroTube.Services.VideoContent.Processing
{
	public class DefaultVideoProcessingPipeline : IVideoProcessingPipeline
	{
		public PipelineState State => _state;

		private PipelineState _state;
		private readonly List<IPipelineStage<DefaultVideoProcessingContext>> _stages = new();
		private readonly ILogger<DefaultVideoProcessingPipeline> _logger;
		private readonly MicroTubeDbContext _db;
		private readonly IFileSystem _fileSystem;
		private readonly IVideoContentRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> _remoteStorage;

		public DefaultVideoProcessingPipeline(
			IConfiguration config,
			ILogger<DefaultVideoProcessingPipeline> logger,
			IEnumerable<VideoProcessingStage> stages,
			ICdnMediaContentAccess mediaCdnAccess,
			MicroTubeDbContext db,
			IFileSystem fileSystem,
			IVideoContentRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> remoteStorage)
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

		public void AddStage(IPipelineStage<DefaultVideoProcessingContext> stage)
		{
			ThrowIfExecuting();
			_stages.Add(stage);
		}


		public void InsertStage(IPipelineStage<DefaultVideoProcessingContext> stage, int index)
		{
			ThrowIfExecuting();
			_stages.Insert(index, stage);
		}

		public void RemoveStage(IPipelineStage<DefaultVideoProcessingContext> stage)
		{
			ThrowIfExecuting();
			_stages.Remove(stage);
		}

		public void RemoveStageAt(int index)
		{
			ThrowIfExecuting();
			_stages.RemoveAt(index);
		}
		public async Task<DefaultVideoProcessingContext> Execute(DefaultVideoProcessingContext? context, CancellationToken cancellationToken = default)
		{
			ThrowIfExecuting();
			if (context == null)
				throw new ArgumentNullException("InitialContext must be set for this Pipeline");
			context.Stopwatch = Stopwatch.StartNew();
			_state = PipelineState.Executing;
			try
			{
				foreach(var stage in _stages)
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
					_state = PipelineState.Error;
					if (context.RemoteCache != null && !string.IsNullOrWhiteSpace(context.RemoteCache.VideoFileLocation))
						await DeleteAllRemoteData(context.RemoteCache.VideoFileLocation);
					if (context.UploadProgress != null)
					{
						_db.Update(context.UploadProgress);
						context.UploadProgress.Status = VideoUploadStatus.Fail;
						if (context.UploadProgress.Message == null)
							context.UploadProgress.Message = "Unknown error";
						await _db.SaveChangesAsync();
					}
				}
				catch { }
				throw;
			}
			finally
			{
				_state = PipelineState.Finished;
				if(context.LocalCache != null && context.LocalCache.WorkingLocation != null)
				{
					ClearLocalCache(context.LocalCache.WorkingLocation);
				}
				if (context.Stopwatch != null)
					context.Stopwatch.Stop();
			}
		}
		private void ThrowIfExecuting()
		{
			if (_state == PipelineState.Executing)
				throw new BackgroundJobException("Operation is not allowed while the pipeline is executing");
		}
		private async Task DeleteAllRemoteData(string remoteLocation)
		{
			await _remoteStorage.DeleteLocation(remoteLocation); //TO DO: make into account retry possibility
		}
		private void ClearLocalCache(string workingLocation)
		{
			_fileSystem.TryDeleteFileOrDirectory(workingLocation);
		}
		
	}
}
