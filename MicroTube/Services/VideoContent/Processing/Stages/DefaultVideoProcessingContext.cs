using MicroTube.Data.Models;
using System.Diagnostics;

namespace MicroTube.Services.VideoContent.Processing.Stages
{
	public class DefaultVideoProcessingContext
	{
		public string? SourceVideoNormalizedName { get; set; }
		public Video? CreatedVideo { get; set; }
		public VideoUploadProgress? UploadProgress { get; set; }
		public SourceVideoRemoteCache? RemoteCache { get; set; }
		public SourceVideoLocalCache? LocalCache { get; set; }
		public Subcontent? Subcontent { get; set; }
		public Cdn? Cdn { get; set; }
		public Stopwatch? Stopwatch { get; set; }
	}
	public class SourceVideoRemoteCache
	{
		public string? VideoFileName { get; set; }
		public string? VideoFileLocation { get; set; }
	}
	public class SourceVideoLocalCache
	{
		public string? VideoFileName { get; set; }
		public string? VideoFileLocation { get; set; }
	}
	public class Subcontent
	{
		public IEnumerable<string>? ThumbnailPaths { get; set; }
	}
	public class Cdn
	{
		public IEnumerable<Uri>? ThumbnailEndpoints { get; set; }
		public Uri? VideoEndpoint { get; set; }
	}
}
