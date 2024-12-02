using Ardalis.GuardClauses;

namespace MicroTube.Services.VideoContent.Processing
{
	public class FFMpegVideoProcessingArgumentsProvider : IVideoProcessingArgumentsProvider
	{
		private readonly IReadOnlyDictionary<string, string> compressionArguments = new Dictionary<string, string>
		{
			{".mp4", "-vf \"scale=-2:{0}\" -threads 3" },
			{".mov", "-vf \"scale=-2:{0}\" -threads 3" },
			{".avi", "-vf \"scale=-2:{0}\" -threads 3" },
			{".wmv", "-vf \"scale=-2:{0}\" -threads 3" },
			{".webm", "-c:v libx264 -c:a aac -b:a 192k -vf \"scale=-2:{0}\" -threads 3" },
		};
		private readonly IReadOnlyDictionary<string, string> thumbnailsArguments = new Dictionary<string, string>
		{
			{".mp4", "-vf \"fps=1/{0},scale={2}:{3}:force_original_aspect_ratio=decrease\" -vsync vfr -threads 1" },
			{".mov", "-vf \"fps=1/{0},scale={2}:{3}:force_original_aspect_ratio=decrease\" -vsync vfr -threads 1" },
			{".avi", "-vf \"fps=1/{0},scale={2}:{3}:force_original_aspect_ratio=decrease\" -vsync vfr -threads 1" },
			{".wmv", "-vf \"fps=1/{0},scale={2}:{3}:force_original_aspect_ratio=decrease\" -vsync vfr -threads 1" },
			{".webm", "-vf \"fps=1/{0},scale={2}:{3}:force_original_aspect_ratio=decrease\" -vsync vfr -threads 1" },
		};
		public string ProvideForCompression(string fileExtension)
		{
			Guard.Against.NullOrWhiteSpace(fileExtension);
			if (!compressionArguments.TryGetValue(fileExtension.ToLower(), out var arg))
				throw new RequiredObjectNotFoundException($"Video processing arguments not found for file {fileExtension}");
			return arg;
		}
		public string ProvideForThumbnails(string fileExtension)
		{
			Guard.Against.NullOrWhiteSpace(fileExtension);
			if (!thumbnailsArguments.TryGetValue(fileExtension.ToLower(), out var arg))
				throw new RequiredObjectNotFoundException($"Video processing arguments not found for file {fileExtension}");
			return arg;
		}
	}
}
