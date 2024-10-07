using FFmpeg.NET;
using MicroTube.Services.ConfigOptions;
using System.IO.Abstractions;

namespace MicroTube.Services.VideoContent.Processing
{
	public class FFMpegVideoThumbnailsService : IVideoThumbnailsService
	{
		private const string FFMPEG_SNAPSHOTS_ARGS = "-vf \"select=bitor(gte(t-prev_selected_t\\,{0})\\,isnan(prev_selected_t)),scale={1}:{2}:force_original_aspect_ratio=decrease\" -vsync 0 -threads 3";

		private readonly ILogger<FFMpegVideoThumbnailsService> _logger;
		private readonly IConfiguration _config;
		private readonly IFileSystem _fileSystem;
		private readonly IVideoAnalyzer _videoAnalyzer;
		private readonly IVideoProcessingArgumentsProvider _argumentsProvider;

		public FFMpegVideoThumbnailsService(
			ILogger<FFMpegVideoThumbnailsService> logger,
			IConfiguration config,
			IFileSystem fileSystem,
			IVideoAnalyzer videoAnalyzer,
			IVideoProcessingArgumentsProvider argumentsProvider)
		{
			_logger = logger;
			_config = config;
			_fileSystem = fileSystem;
			_videoAnalyzer = videoAnalyzer;
			_argumentsProvider = argumentsProvider;
		}

		public async Task<IServiceResult<IEnumerable<string>>> MakeSnapshots(string filePath, string saveToPath, CancellationToken cancellationToken = default)
		{
			try
			{
				if (!_fileSystem.File.Exists(filePath))
					throw new RequiredObjectNotFoundException($"Source file at {filePath} does not exist. Aborting thumbnail generation.");
				if (!_fileSystem.Directory.Exists(saveToPath))
					throw new RequiredObjectNotFoundException($"Output location at {saveToPath} does not exist. Aborting thumbnail generation.");
				VideoProcessingOptions processingOptions = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
				string ffmpegCustomArgs = string.Format(FFMPEG_SNAPSHOTS_ARGS,
					processingOptions.SnapshotsIntervalSeconds, processingOptions.SnapshotsWidth, processingOptions.SnapshotsHeight);
				var options = new ConversionOptions
				{
					ExtraArguments = ffmpegCustomArgs
				};
				var inputFile = new InputFile(filePath);
				var outputFile = new OutputFile(_fileSystem.Path.Join(saveToPath, "snapshot%4d.jpg"));
				var ffmpeg = new Engine(_config["FFmpegLocation"]);
				await ffmpeg.ConvertAsync(inputFile, outputFile, options, cancellationToken);
				var files = _fileSystem.Directory.GetFiles(saveToPath);
				var snapshotPaths = files.Where(_ => _.Contains("snapshot")); //TO DO: this could use more robust filtering
				return ServiceResult<IEnumerable<string>>.Success(snapshotPaths);
			}
			catch(Exception e)
			{
				return e.ExceptionToErrorResult<IEnumerable<string>>();
			}
			
			
		}
		public async Task<IServiceResult<IEnumerable<string>>> MakeThumbnails(string filePath, string saveToPath, CancellationToken cancellationToken = default)
		{
			try
			{
				if (!_fileSystem.File.Exists(filePath))
					throw new RequiredObjectNotFoundException($"Source file at {filePath} does not exist. Aborting thumbnail generation.");
				if (!_fileSystem.Directory.Exists(saveToPath))
					throw new RequiredObjectNotFoundException($"Output location at {saveToPath} does not exist. Aborting thumbnail generation.");
				VideoProcessingOptions processingOptions = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
				var videoAnalysis = await _videoAnalyzer.Analyze(filePath);
				double framesCount = videoAnalysis.FrameCount;
				var inputFile = new InputFile(filePath);
				string processingArguments = _argumentsProvider.ProvideForThumbnails(Path.GetExtension(filePath));
				string ffmpegCustomArgs = string.Format(processingArguments,
					framesCount, processingOptions.ThumbnailsAmount, processingOptions.ThumbnailsWidth, processingOptions.ThumbnailsHeight);
				var options = new ConversionOptions
				{
					ExtraArguments = ffmpegCustomArgs
				};
				var outputFile = new OutputFile(_fileSystem.Path.Join(saveToPath, "thumbnail%4d.jpg"));
				var ffmpeg = new Engine(_config.GetRequiredValue("FFmpegLocation"));
				await ffmpeg.ConvertAsync(inputFile, outputFile, options, cancellationToken);
				var files = _fileSystem.Directory.GetFiles(saveToPath);
				var thumbnailPaths = files.Where(_ => _.Contains("thumbnail"));//TO DO: this could use more robust filtering
				return ServiceResult<IEnumerable<string>>.Success(thumbnailPaths);
			}
			catch(Exception e)
			{
				return e.ExceptionToErrorResult<IEnumerable<string>>();
			}
			
		} 
	}
}
