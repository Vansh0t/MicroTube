using FFmpeg.NET;
using MicroTube.Services.ConfigOptions;

namespace MicroTube.Services.VideoContent.Processing
{
	public class FFMpegVideoCompressionService : IVideoCompressionService
	{
		private const string FFMPEG_COMPRESSION_ARGS = "-vf \"scale=-2:{0}\" -threads 3";
		private readonly IConfiguration _config;
		private readonly IVideoAnalyzer _analyzer;

		public FFMpegVideoCompressionService(IConfiguration config, IVideoAnalyzer analyzer)
		{
			_config = config;
			_analyzer = analyzer;
		}

		public async Task<IServiceResult<IDictionary<int, string>>> CompressToQualityTiers(IEnumerable<int> tiers, string sourcePath, string saveToPath, CancellationToken cancellationToken = default)
		{
			var analyzeResult = await _analyzer.Analyze(sourcePath, cancellationToken);
			if (int.TryParse(analyzeResult.FrameSize.Split('x').LastOrDefault(), out int parsedHeight))
			{
				Dictionary<int, string> outputPaths = new Dictionary<int, string>();
				foreach (var tier in tiers)
				{
					if (tier > parsedHeight)
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
				return ServiceResult<IDictionary<int, string>>.Fail(500, $"Failed to parse source video height. Frame size: {analyzeResult.FrameSize}");
			}
		}
		public async Task<IServiceResult<string>> CompressToTier(int tier, string sourcePath, string outputPath, CancellationToken cancellationToken = default)
		{
			try
			{
				string ffmpegCustomArgs = string.Format(FFMPEG_COMPRESSION_ARGS, tier);
				var options = new ConversionOptions
				{
					ExtraArguments = ffmpegCustomArgs
				};
				var inputFile = new InputFile(sourcePath);

				string outputFileName = BuildOutputFileName(tier, sourcePath);
				outputPath = Path.Join(outputPath, outputFileName);
				var outputFile = new OutputFile(outputPath);
				var ffmpeg = new Engine(_config.GetRequiredValue("FFmpegLocation"));
				var result = await ffmpeg.ConvertAsync(inputFile, outputFile, options, cancellationToken);
				if(result == null)
				{
					return ServiceResult<string>.Fail(500, $"Failed to convert to quality tier. Source: {sourcePath}, Tier: {tier}, OutputPath: {outputPath}");
				}
				return ServiceResult<string>.Success(outputPath);
			}
			catch (Exception e)
			{
				return e.ExceptionToErrorResult<string>();
			}

		}
		private string BuildOutputFileName(int tier, string sourceFilePath)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFilePath);
			string fileExtension = Path.GetExtension(sourceFilePath);
			string tierFileName = $"{fileNameWithoutExtension}_{tier}{fileExtension}";
			return tierFileName;
		}
	}
}
