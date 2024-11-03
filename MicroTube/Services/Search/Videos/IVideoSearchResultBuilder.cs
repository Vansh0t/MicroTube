namespace MicroTube.Services.Search.Videos
{
	public interface IVideoSearchResultBuilder<TResponse>
	{
		VideoSearchResult Build(TResponse response, string? meta);
	}
}