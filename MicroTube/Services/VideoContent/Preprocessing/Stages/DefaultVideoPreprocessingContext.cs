using MicroTube.Data.Models;
using MicroTube.Services.VideoContent.Processing.Stages;

namespace MicroTube.Services.VideoContent.Preprocessing.Stages
{
    public class DefaultVideoPreprocessingContext
	{
		public VideoUploadProgress? UploadProgress { get; set; }
		public VideoProcessingRemoteCache? RemoteCache { get; set; }
		public required VideoPreprocessingData PreprocessingData { get; set; }
	}
}
