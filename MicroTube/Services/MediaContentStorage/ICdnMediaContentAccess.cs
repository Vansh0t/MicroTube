namespace MicroTube.Services.MediaContentStorage
{
	public interface ICdnMediaContentAccess
	{
		Task<IServiceResult> UploadVideo(Stream stream, string fileName, CancellationToken cancellationToken = default);
	}
}