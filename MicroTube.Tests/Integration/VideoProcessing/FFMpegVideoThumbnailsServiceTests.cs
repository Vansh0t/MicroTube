using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.VideoContent.Processing;
using NSubstitute;
using System.IO.Abstractions;
using System.Runtime.InteropServices;

namespace MicroTube.Tests.Integration.VideoProcessing
{
	public class FFMpegVideoThumbnailsServiceTests:IDisposable
	{
		private readonly IConfigurationRoot config;
		private readonly string videoTestsTempLocation;

		public FFMpegVideoThumbnailsServiceTests()
		{
			bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
			config = new ConfigurationBuilder()
				//current FFMpeg package is a bit dumb, so make sure we look for a .exe file on Windows
				.AddInMemoryCollection(new Dictionary<string, string?> { { "FFmpegLocation", isWindows ? "ffmpeg.exe" : "ffmpeg" } })
				.AddConfigObject(VideoProcessingOptions.KEY, new VideoProcessingOptions("", 0, 0, "", 0, 5, 320, 180, 10, 720, 400, 480, 240))
				.Build();
			videoTestsTempLocation = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TEST_VIDEOS_TEMP_LOCATION, Guid.NewGuid().ToString().Replace("-", ""));
			Directory.CreateDirectory(videoTestsTempLocation);
		}
		[Fact]
		public async Task MakeThumbnails_Success()
		{
			var thumbnailsService = new FFMpegVideoThumbnailsService(
				Substitute.For<ILogger<FFMpegVideoThumbnailsService>>(),
				config,
				new FileSystem(),
				new FFMpegVideoAnalyzer(config));
			var sourcePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TEST_VIDEO_20S_480P_24FPS_LOCATION);
			var result = await thumbnailsService.MakeThumbnails(sourcePath, videoTestsTempLocation);
			Assert.False(result.IsError);
			var thumbnailPaths = result.GetRequiredObject();
			Assert.Equal(config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY).ThumbnailsAmount, thumbnailPaths.Count());
			foreach(var thumbnailPath in thumbnailPaths)
			{
				Assert.True(File.Exists(thumbnailPath));
			}
		}
		[Fact]
		public async Task MakeThumbnails_InvalidPathFail()
		{
			var sourcePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TEST_VIDEO_20S_480P_24FPS_LOCATION);
			var thumbnailsService = new FFMpegVideoThumbnailsService(
				Substitute.For<ILogger<FFMpegVideoThumbnailsService>>(),
				config,
				new FileSystem(),
				new FFMpegVideoAnalyzer(config));
			var result = await thumbnailsService.MakeThumbnails(sourcePath, "Invalid/path");
			Assert.True(result.IsError);
			Assert.Equal(500, result.Code);
			result = await thumbnailsService.MakeThumbnails("Invalid/path", videoTestsTempLocation);
			Assert.True(result.IsError);
			Assert.Equal(500, result.Code);

		}
		[Fact]
		public async Task MakeSnapshots_Success()
		{
			var thumbnailsService = new FFMpegVideoThumbnailsService(
				Substitute.For<ILogger<FFMpegVideoThumbnailsService>>(),
				config,
				new FileSystem(),
				new FFMpegVideoAnalyzer(config));
			var sourcePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TEST_VIDEO_20S_480P_24FPS_LOCATION);
			var result = await thumbnailsService.MakeSnapshots(sourcePath, videoTestsTempLocation);
			Assert.False(result.IsError);
			var snapshotPaths = result.GetRequiredObject();
			Assert.Equal(4, snapshotPaths.Count());
			foreach (var thumbnailPath in snapshotPaths)
			{
				Assert.True(File.Exists(thumbnailPath));
			}
		}
		[Fact]
		public async Task MakeSnapshots_InvalidPathFail()
		{
			var sourcePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TEST_VIDEO_20S_480P_24FPS_LOCATION);
			var thumbnailsService = new FFMpegVideoThumbnailsService(
				Substitute.For<ILogger<FFMpegVideoThumbnailsService>>(),
				config,
				new FileSystem(),
				new FFMpegVideoAnalyzer(config));
			var result = await thumbnailsService.MakeSnapshots(sourcePath, "Invalid/path");
			Assert.True(result.IsError);
			Assert.Equal(500, result.Code);
			result = await thumbnailsService.MakeSnapshots("Invalid/path", videoTestsTempLocation);
			Assert.True(result.IsError);
			Assert.Equal(500, result.Code);

		}
		public void Dispose()
		{
			Directory.Delete(videoTestsTempLocation, true);
		}
	}
}
