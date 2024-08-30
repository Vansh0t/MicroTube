using FFmpeg.NET.Extensions;
using Microsoft.Extensions.Configuration;
using MicroTube.Services.VideoContent.Processing;
using System.Runtime.InteropServices;

namespace MicroTube.Tests.Integration.VideoProcessing
{
	public class FFMpegVideoAnalyzerTests
	{
		[Fact]
		public async Task Analyze_Success()
		{
			bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows); 
			var config = new ConfigurationBuilder()
				//current FFMpeg package is a bit dumb, so make sure we look for a .exe file on Windows
				.AddInMemoryCollection(new Dictionary<string, string?> { { "FFmpegLocation", isWindows? "ffmpeg.exe":"ffmpeg" } })
				.Build();
			var analyzer = new FFMpegVideoAnalyzer(config);
			var path = Path.Join( AppDomain.CurrentDomain.BaseDirectory, Constants.TEST_VIDEO_20S_1080P_24FPS_LOCATION);
			var result = await analyzer.Analyze(path);
			Assert.Equal(20, result.LengthSeconds);
			Assert.Equal("1920x1080", result.FrameSize);
			Assert.Equal(24, result.Fps, 0.1f);
			Assert.Equal(479f, result.FrameCount, 1);
		}
	}
}
