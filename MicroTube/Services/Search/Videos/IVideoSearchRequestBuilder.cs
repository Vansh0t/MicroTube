namespace MicroTube.Services.Search.Videos
{
	public interface IVideoSearchRequestBuilder<TResult>
	{
		TResult Build(VideoSearchParameters parameters, string? meta);
	}
}