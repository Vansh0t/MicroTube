namespace MicroTube.Services.MediaContentStorage
{
	public interface ICdnMediaContentAccess
	{
		Task<IServiceResult> DeleteAllVideoData(string videoFileName, CancellationToken cancellationToken = default);
		Task<IServiceResult<Uri>> UploadVideo(Stream stream, string fileName, string location, CancellationToken cancellationToken = default);
		Task<IServiceResult<IEnumerable<Uri>>> UploadVideoSubcontent(string fromPath, string location, CancellationToken cancellationToken = default);
		public Task<IServiceResult<string>> CreateVideoLocation(string videoFileName, CancellationToken cancellationToken = default);
	}
}