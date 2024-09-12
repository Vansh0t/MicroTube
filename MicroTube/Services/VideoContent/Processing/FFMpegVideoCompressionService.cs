using FFmpeg.NET;
using System.IO.Abstractions;

namespace MicroTube.Services.VideoContent.Processing
{
	public class FFMpegVideoCompressionService : IVideoCompressionService
	{
		private const string FFMPEG_COMPRESSION_ARGS = "-vf \"scale=-2:{0}\" -threads 3";
		private readonly IConfiguration _config;
		private readonly IVideoAnalyzer _analyzer;
		private readonly IFileSystem _fileSystem;

		public FFMpegVideoCompressionService(IConfiguration config, IVideoAnalyzer analyzer, IFileSystem fileSystem)
		{
			_config = config;
			_analyzer = analyzer;
			_fileSystem = fileSystem;
		}

		public async Task<IServiceResult<IDictionary<int, string>>> CompressToQualityTiers(IEnumerable<int> tiers, string sourcePath, string saveToPath, CancellationToken cancellationToken = default)
		{
			var analyzeResult = await _analyzer.Analyze(sourcePath, cancellationToken);
			if (analyzeResult.FrameHeight > 0)
			{
				Dictionary<int, string> outputPaths = new Dictionary<int, string>();
				foreach (var tier in tiers)
				{
					if (tier > analyzeResult.FrameHeight)
						continue;
					var tierResult = await CompressToTier(tier, sourcePath, saveToPath, cancellationToken);
					if (tierResult.IsError)
					{
						return ServiceResult<IDictionary<int, string>>.Fail(500, $"Failed to compress video for quality tier {tier}. {tierResult.Error}");
					}
					outputPaths.Add(tier, tierResult.GetRequiredObject());
				}
				return ServiceResult<IDictionary<int, string>>.Success(outputPaths);
			}
			else
			{
				return ServiceResult<IDictionary<int, string>>.Fail(500, $"Invalid video source height. Frame size: {analyzeResult.FrameSize}");
			}
		}
		public async Task<IServiceResult<string>> CompressToTier(int tier, string sourcePath, string outputLocation, CancellationToken cancellationToken = default)
		{
			try
			{
				var inputFile = new InputFile(sourcePath);
				if (!inputFile.FileInfo.Exists)
					throw new RequiredObjectNotFoundException($"Source file at {sourcePath} does not exist. Aborting compression.");
				if(!_fileSystem.Directory.Exists(outputLocation))
					throw new RequiredObjectNotFoundException($"Output location at {outputLocation} does not exist. Aborting compression.");
				var analyzeResult = await _analyzer.Analyze(sourcePath, cancellationToken);
				if (tier > analyzeResult.FrameHeight)
					throw new ArgumentException("Tier cannot be larger than source height. Aborting compression.");
				string ffmpegCustomArgs = string.Format(FFMPEG_COMPRESSION_ARGS, tier);
				var options = new ConversionOptions
				{
					ExtraArguments = ffmpegCustomArgs
				};
				
				string outputFileName = BuildOutputFileName(tier, sourcePath);
				outputLocation = _fileSystem.Path.Join(outputLocation, outputFileName);
				var outputFile = new OutputFile(outputLocation);
				var ffmpeg = new Engine(_config.GetRequiredValue("FFmpegLocation"));
				MediaFile result = await ffmpeg.ConvertAsync(inputFile, outputFile, options, cancellationToken);
				if(result == null || !result.FileInfo.Exists)
				{
					 throw new ExternalServiceException($"Failed to convert to quality tier. Source: {sourcePath}, Tier: {tier}, OutputPath: {outputLocation}");
				}
				return ServiceResult<string>.Success(outputLocation);
			}
			catch (Exception e)
			{
				return e.ExceptionToErrorResult<string>();
			}

		}
		private string BuildOutputFileName(int tier, string sourceFilePath)
		{
			string fileNameWithoutExtension = _fileSystem.Path.GetFileNameWithoutExtension(sourceFilePath);
			string fileExtension = _fileSystem.Path.GetExtension(sourceFilePath);
			string tierFileName = $"{fileNameWithoutExtension}_{tier}{fileExtension}";
			return tierFileName;
		}
	}
}
