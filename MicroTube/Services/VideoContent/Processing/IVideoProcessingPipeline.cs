using MicroTube.Services.Base;
using MicroTube.Services.VideoContent.Processing.Stages;

namespace MicroTube.Services.VideoContent.Processing
{
	public interface IVideoProcessingPipeline: IPipeline<DefaultVideoProcessingContext>
	{
		
	}
}