namespace MicroTube.Services.VideoContent.Processing
{
	public interface IVideoPreprocessingPipeline<TData, TUploadResult>
	{
		Task<IServiceResult<TUploadResult>> PreprocessVideo(TData data);
	}
}