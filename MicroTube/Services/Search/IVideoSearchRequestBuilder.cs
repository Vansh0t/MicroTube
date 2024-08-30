namespace MicroTube.Services.Search
{
	public interface IVideoSearchRequestBuilder<TResult>
	{
		TResult Build(VideoSearchParameters parameters, string? meta);
	}
}