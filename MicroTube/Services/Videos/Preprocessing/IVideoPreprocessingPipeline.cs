using MicroTube.Services.Base;
using MicroTube.Services.VideoContent.Preprocessing.Stages;

namespace MicroTube.Services.VideoContent.Preprocessing
{
	public interface IVideoPreprocessingPipeline : IPipeline<DefaultVideoPreprocessingContext>
	{
	}
}