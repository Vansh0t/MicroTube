using MicroTube.Data.Models;

namespace MicroTube.Services.MediaContentStorage
{
    public interface IVideoContentLocalStorage
    {
        Task<IServiceResult<FileMeta>> Save(Stream stream, string fileName, CancellationToken cancellationToken = default);
        bool TryDelete(string fullFilePath);
    }
}