using Microsoft.Extensions.Configuration;
using MicroTube.Services.VideoContent.Processing;
using System.IO.Abstractions;
using System.Runtime.InteropServices;

namespace MicroTube.Tests.Integration.VideoProcessing
{
	public class FFMpegVideoCompressionServiceTests:IDisposable
	{
		private readonly IConfigurationRoot config;
		private readonly string videoTestsTempLocation;

		public FFMpegVideoCompressionServiceTests()
		{
			bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
			config = new ConfigurationBuilder()
				//current FFMpeg package is a bit dumb, so make sure we look for a .exe file on Windows
				.AddInMemoryCollection(new Dictionary<string, string?> { { "FFmpegLocation", isWindows ? "ffmpeg.exe" : "ffmpeg" } })
				.Build();
			videoTestsTempLocation = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TEST_VIDEOS_TEMP_LOCATION, Guid.NewGuid().ToString().Replace("-", ""));
			Directory.CreateDirectory(videoTestsTempLocation);
		}

		[Theory]
		[InlineData(144)]
		[InlineData(240)]
		[InlineData(360)]
		[InlineData(480)]
		[InlineData(720)]
		[InlineData(1080)]
		public async Task CompressToTier_Success(int tier)
		{
			var analyzer = new FFMpegVideoAnalyzer(config);
			var sourcePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TEST_VIDEO_20S_1080P_24FPS_LOCATION);
			var compressionService = new FFMpegVideoCompressionService(config, analyzer, new FileSystem());
			var result = await compressionService.CompressToTier(tier, sourcePath, videoTestsTempLocation);
			Assert.False(result.IsError);
			Assert.NotEmpty(result.GetRequiredObject());
			var sourceAnalysis =await analyzer.Analyze(sourcePath);
			var resultAnalysis =await analyzer.Analyze(result.GetRequiredObject());
			Assert.Equal(sourceAnalysis.Format, resultAnalysis.Format);
			Assert.Equal(sourceAnalysis.Fps, resultAnalysis.Fps);
			Assert.Equal(sourceAnalysis.FrameCount, resultAnalysis.FrameCount);
			Assert.Equal(tier, int.Parse(resultAnalysis.FrameSize.Split("x")[1]));
		}
		[Fact]
		public async Task CompressToTier_InvalidPathFail()
		{
			var analyzer = new FFMpegVideoAnalyzer(config);
			var sourcePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TEST_VIDEO_20S_1080P_24FPS_LOCATION);
			var compressionService = new FFMpegVideoCompressionService(config, analyzer, new FileSystem());
			var result = await compressionService.CompressToTier(144, "Invalid/path", videoTestsTempLocation);
			Assert.True(result.IsError);
			Assert.Equal(500, result.Code);
			result = await compressionService.CompressToTier(144, sourcePath, "Invalid/path");
			Assert.True(result.IsError);
			Assert.Equal(500, result.Code);

		}
		[Fact]
		public async Task CompressToTier_TierTooHighFail()
		{
			var analyzer = new FFMpegVideoAnalyzer(config);
			var sourcePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TEST_VIDEO_20S_1080P_24FPS_LOCATION);
			var compressionService = new FFMpegVideoCompressionService(config, analyzer, new FileSystem());
			var result = await compressionService.CompressToTier(1440, sourcePath, videoTestsTempLocation);
			Assert.True(result.IsError);
			Assert.Equal(500, result.Code);
		}
		[Fact]
		public async Task CompressToQualityTiers_Success()
		{
			List<int> tiers = new List<int> { 144, 240, 360, 480, 720, 1080, 1440 };
			var analyzer = new FFMpegVideoAnalyzer(config);
			var sourcePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TEST_VIDEO_20S_1080P_24FPS_LOCATION);
			var compressionService = new FFMpegVideoCompressionService(config, analyzer, new FileSystem());
			var result = await compressionService.CompressToQualityTiers(tiers, sourcePath, videoTestsTempLocation);
			Assert.False(result.IsError);
			Assert.NotEmpty(result.GetRequiredObject());
			var sourceAnalysis = await analyzer.Analyze(sourcePath);
			Assert.DoesNotContain(1440, result.GetRequiredObject());
			foreach(var qualityPath in result.GetRequiredObject())
			{
				var resultAnalysis = await analyzer.Analyze(qualityPath.Value);
				Assert.Equal(sourceAnalysis.Format, resultAnalysis.Format);
				Assert.Equal(sourceAnalysis.Fps, resultAnalysis.Fps);
				Assert.Equal(sourceAnalysis.FrameCount, resultAnalysis.FrameCount);
				Assert.Equal(qualityPath.Key, resultAnalysis.FrameHeight);
			}
			
		}
		public void Dispose()
		{
			Directory.Delete(videoTestsTempLocation, true);
		}
	}
}
