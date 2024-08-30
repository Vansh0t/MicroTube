using FFmpeg.NET;
using MicroTube.Services.ConfigOptions;
using System.IO.Abstractions;

namespace MicroTube.Services.VideoContent.Processing
{
	public class FFMpegVideoThumbnailsService : IVideoThumbnailsService
	{
		private const string FFMPEG_SNAPSHOTS_ARGS = "-vf \"select=bitor(gte(t-prev_selected_t\\,{0})\\,isnan(prev_selected_t)),scale={1}:{2}:force_original_aspect_ratio=decrease\" -vsync 0 -threads 3";
		private const string FFMPEG_THUMBNAILS_ARGS = "-vf \"thumbnail=n={0}/{1},scale={2}:{3}:force_original_aspect_ratio=decrease\" -vsync 0 -threads 3";

		private readonly ILogger<FFMpegVideoThumbnailsService> _logger;
		private readonly IConfiguration _config;
		private readonly IFileSystem _fileSystem;

		public FFMpegVideoThumbnailsService(ILogger<FFMpegVideoThumbnailsService> logger, IConfiguration config, IFileSystem fileSystem)
		{
			_logger = logger;
			_config = config;
			_fileSystem = fileSystem;
		}

		public async Task<IServiceResult<IEnumerable<string>>> MakeSnapshots(string filePath, string saveToPath, CancellationToken cancellationToken = default)
		{
			try
			{
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
				VideoProcessingOptions processingOptions = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
				var inputFile = new InputFile(filePath);
				var ffprobe = new Engine(_config.GetRequiredValue("FFmpegLocation"));
				var videoAnylisis = await ffprobe.GetMetaDataAsync(inputFile, cancellationToken);
				double framesCount = Math.Floor(videoAnylisis.Duration.TotalSeconds * videoAnylisis.VideoData.Fps);
				string ffmpegCustomArgs = string.Format(FFMPEG_THUMBNAILS_ARGS,
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
