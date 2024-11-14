namespace MicroTube.Services.VideoContent.Views
{
	public interface IVideoViewsAggregatorService
	{
		Task Aggregate();
		Task<IServiceResult> CreateViewForAggregation(string videoId, string ip);
	}
}