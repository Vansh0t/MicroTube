using FFmpeg.NET;
using MicroTube.Services.ConfigOptions;

namespace MicroTube.Services.VideoContent.Processing
{
	public class FFMpegVideoThumbnailsService : IVideoThumbnailsService
	{
		private const string FFMPEG_SNAPSHOTS_ARGS = "-vf \"select=bitor(gte(t-prev_selected_t\\,{0})\\,isnan(prev_selected_t)),scale={1}:{2}:force_original_aspect_ratio=decrease\" -vsync 0";
		private const string FFMPEG_THUMBNAILS_ARGS = "-vf \"thumbnail=n={0}/{1},scale={2}:{3}:force_original_aspect_ratio=decrease\" -vsync 0";

		private readonly ILogger<FFMpegVideoThumbnailsService> _logger;
		private readonly IConfiguration _config;

		public FFMpegVideoThumbnailsService(ILogger<FFMpegVideoThumbnailsService> logger, IConfiguration config)
		{
			_logger = logger;
			_config = config;
		}

		public async Task<IServiceResult<IEnumerable<string>>> MakeSnapshots(string filePath, string saveToPath, CancellationToken cancellationToken = default)
		{
			//TODO: cover potential error explanations
			VideoProcessingOptions processingOptions = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
			string snapshotsDirectory = EnsureFileSaveTargetDirectory(filePath, saveToPath);
			string ffmpegCustomArgs = string.Format(FFMPEG_SNAPSHOTS_ARGS, 
				processingOptions.SnapshotsIntervalSeconds, processingOptions.SnapshotsWidth, processingOptions.SnapshotsHeight);
			var options = new ConversionOptions 
			{ 
				ExtraArguments = ffmpegCustomArgs
			};
			var inputFile = new InputFile(filePath);
			var outputFile = new OutputFile(Path.Join(snapshotsDirectory, "snapshot%4d.jpg"));
			var ffmpeg = new Engine(_config["FFmpegLocation"]);
			await ffmpeg.ConvertAsync(inputFile, outputFile, options, cancellationToken);
			var files = Directory.GetFiles(snapshotsDirectory);
			var snapshotPaths = files.Where(_ => _.Contains("snapshot")); //TODO: this could use more robust filtering
			return ServiceResult<IEnumerable<string>>.Success(snapshotPaths);
		}
		public async Task<IServiceResult<IEnumerable<string>>> MakeThumbnails(string filePath, string saveToPath, CancellationToken cancellationToken = default)
		{
			//TODO: cover potential error explanations
			VideoProcessingOptions processingOptions = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
			string thumbnailsDirectory = EnsureFileSaveTargetDirectory(filePath, saveToPath);
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
			var outputFile = new OutputFile(Path.Join(thumbnailsDirectory, "thumbnail%4d.jpg"));
			var ffmpeg = new Engine(_config.GetRequiredValue("FFmpegLocation"));
			await ffmpeg.ConvertAsync(inputFile, outputFile, options, cancellationToken);
			var files = Directory.GetFiles(thumbnailsDirectory);
			var thumbnailPaths = files.Where(_=>_.Contains("thumbnail"));//TODO: this could use more robust filtering
			return ServiceResult<IEnumerable<string>>.Success(thumbnailPaths);
		} 
		private string EnsureFileSaveTargetDirectory(string filePath, string saveToPath)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
			string directory = Path.Join(saveToPath, fileNameWithoutExtension);
			Directory.CreateDirectory(directory);
			return directory;
		}
	}
}
