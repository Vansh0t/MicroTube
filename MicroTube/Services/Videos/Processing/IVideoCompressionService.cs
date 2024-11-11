namespace MicroTube.Services.VideoContent.Processing
{
	public interface IVideoCompressionService
	{
		Task<IServiceResult<IDictionary<int, string>>> CompressToQualityTiers(IEnumerable<int> tiers, string filePath, string saveToPath, CancellationToken cancellationToken = default);
		Task<IServiceResult<string>> CompressToTier(int tier, string sourcePath, string outputLocation, CancellationToken cancellationToken = default);
	}
}