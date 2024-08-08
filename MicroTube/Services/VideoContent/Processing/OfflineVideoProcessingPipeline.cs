using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.Base;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.MediaContentStorage;
using MicroTube.Services.VideoContent.Processing.Stages;
using System.Diagnostics;

namespace MicroTube.Services.VideoContent.Processing
{
	public class OfflineVideoProcessingPipeline : IVideoProcessingPipeline
	{
		public PipelineState State => _state;

		private PipelineState _state;
		private List<IPipelineStage<DefaultVideoProcessingContext>> _stages = new();
		private IConfiguration _config;
		private ILogger<OfflineVideoProcessingPipeline> _logger;
		private ICdnMediaContentAccess _mediaCdnAccess;
		private IVideoDataAccess _videoDataAccess;
		private IVideoContentLocalStorage _localStorage;

		public OfflineVideoProcessingPipeline(
			IConfiguration config,
			ILogger<OfflineVideoProcessingPipeline> logger,
			IEnumerable<VideoProcessingStage> stages,
			ICdnMediaContentAccess mediaCdnAccess,
			IVideoDataAccess videoDataAccess,
			IVideoContentLocalStorage localStorage)
		{
			_config = config;
			_logger = logger;
			foreach (var stage in stages)
			{
				AddStage(stage);
			}
			_mediaCdnAccess = mediaCdnAccess;
			_videoDataAccess = videoDataAccess;
			_localStorage = localStorage;
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
					_logger.LogInformation("Executing video offline processing stage: " + stage.GetType());
					await stage.Execute(context, cancellationToken);
				}
				context.Stopwatch.Stop();
				return context;
			} 
			catch
			{
				try
				{
					if (context.RemoteCache != null && !string.IsNullOrWhiteSpace(context.RemoteCache.VideoFileName))
						await RevertProcessingOperation(context.RemoteCache.VideoFileName);
					if (context.UploadProgress != null)
					{
						context.UploadProgress.Status = VideoUploadStatus.Fail;
						if (context.UploadProgress.Message == null)
							context.UploadProgress.Message = "Unknown error";
						await _videoDataAccess.UpdateUploadProgress(context.UploadProgress);
					}
				}
				catch { }
				throw;
			}
			finally
			{
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
		private async Task RevertProcessingOperation(string videoFileName)
		{
			await _mediaCdnAccess.DeleteAllVideoData(videoFileName);
		}
		private void ClearLocalCache(string workingLocation)
		{
		    _localStorage.TryDelete(workingLocation);
		}
		
	}
}
