using MicroTube.Data.Models;

namespace MicroTube.Services.VideoContent
{
	public interface IVideoContentLocalStorage
	{
		Task<IServiceResult<FileMeta>> Save(Stream stream, string fileName, CancellationToken cancellationToken = default);
		bool TryDelete(string fullFilePath);
	}
}