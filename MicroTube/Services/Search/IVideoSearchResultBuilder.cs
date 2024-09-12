namespace MicroTube.Services.Search
{
	public interface IVideoSearchResultBuilder<TResponse>
	{
		VideoSearchResult Build(TResponse response, string? meta);
	}
}