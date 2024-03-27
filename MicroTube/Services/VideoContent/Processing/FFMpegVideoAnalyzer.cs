using FFmpeg.NET;

namespace MicroTube.Services.VideoContent.Processing
{
	public class FFMpegVideoAnalyzer : IVideoAnalyzer
	{
		private readonly IConfiguration _config;

		public FFMpegVideoAnalyzer(IConfiguration config)
		{
			_config = config;
		}

		public async Task<VideoFileMetaData> Analyze(string filePath, CancellationToken cancellationToken)
		{
			var inputFile = new InputFile(filePath);
			var ffprobe = new Engine(_config.GetRequiredValue("FFmpegLocation"));
			var videoAnylisis = await ffprobe.GetMetaDataAsync(inputFile, cancellationToken);
			int lengthSeconds = (int)videoAnylisis.Duration.TotalSeconds;
			int frameCount = (int)(lengthSeconds * videoAnylisis.VideoData.Fps);
			return new VideoFileMetaData
			{
				Format = videoAnylisis.FileInfo.Extension,
				FrameCount = frameCount,
				LengthSeconds = lengthSeconds,
				Fps = (int)videoAnylisis.VideoData.Fps,
				FrameSize = videoAnylisis.VideoData.FrameSize
			};
		}
	}
}
