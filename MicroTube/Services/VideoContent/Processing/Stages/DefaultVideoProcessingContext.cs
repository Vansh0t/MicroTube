using MicroTube.Data.Models;
using System.Diagnostics;

namespace MicroTube.Services.VideoContent.Processing.Stages
{
	public class DefaultVideoProcessingContext
	{
		public required string SourceVideoNameWithoutExtension { get; set; }
		public Video? CreatedVideo { get; set; }
		public VideoUploadProgress? UploadProgress { get; set; }
		public VideoProcessingRemoteCache? RemoteCache { get; set; }
		public VideoProcessingLocalCache? LocalCache { get; set; }
		public Cdn? Cdn { get; set; }
		public Stopwatch? Stopwatch { get; set; }
	}
	public class VideoProcessingRemoteCache
	{
		public required string VideoFileName { get; set; }
		public required string VideoFileLocation { get; set; }
	}
	public class VideoProcessingLocalCache
	{
		public required string SourceFileName { get; set; }
		public required string SourceLocation { get; set; }
		public string SourcePath => Path.Join(SourceLocation, SourceFileName);
		public required string WorkingLocation { get; set; }
		public required string ThumbnailsLocation { get; set; }
		public required string QualityTiersLocation { get; set; }
	}
	public class Cdn
	{
		public IEnumerable<Uri>? ThumbnailEndpoints { get; set; }
		public IEnumerable<Uri>? VideoEndpoints { get; set; }
	}
}
