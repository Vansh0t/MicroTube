namespace MicroTube.Services.MediaContentStorage
{
	public interface ICdnMediaContentAccess
	{
		Task<IServiceResult> DeleteAllVideoData(string videoFileName, CancellationToken cancellationToken = default);
		Task<IServiceResult<Uri>> UploadVideo(Stream stream, string fileName, CancellationToken cancellationToken = default);
		Task<IServiceResult<IEnumerable<Uri>>> UploadVideoSubcontent(string fromPath, string videoFileName, CancellationToken cancellationToken = default);
	}
}