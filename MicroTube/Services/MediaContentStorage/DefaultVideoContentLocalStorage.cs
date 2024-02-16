using MicroTube.Data.Models;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.VideoContent;

namespace MicroTube.Services.MediaContentStorage
{
    public class DefaultVideoContentLocalStorage : IVideoContentLocalStorage
    {
        private readonly ILogger<DefaultVideoContentLocalStorage> _logger;
        private readonly IConfiguration _config;
        private readonly IVideoNameGenerator _videoNameGenerator;

        public DefaultVideoContentLocalStorage(IConfiguration config, IVideoNameGenerator videoNameGenerator, ILogger<DefaultVideoContentLocalStorage> logger)
        {
            _config = config;
            _videoNameGenerator = videoNameGenerator;
            _logger = logger;
        }
        public async Task<IServiceResult<FileMeta>> Save(Stream stream, string fileName, CancellationToken cancellationToken = default)
        {
            VideoContentUploadOptions options = _config
                .GetRequiredSection(VideoContentUploadOptions.KEY)
                .GetRequired<VideoContentUploadOptions>();
            string fileExtension = Path.GetExtension(fileName);
            string videoFileName = _videoNameGenerator.GenerateVideoName() + fileExtension;
            string fullPath = Path.Join(options.AbsoluteLocalStoragePath, videoFileName);
            Directory.CreateDirectory(options.AbsoluteLocalStoragePath);
            using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            try
            {
                await stream.CopyToAsync(fileStream, options.LocalStorageUploadBufferSizeBytes, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to save a video file locally.");
                TryDelete(fullPath);
                return ServiceResult<FileMeta>.FailInternal();
            }
            return ServiceResult<FileMeta>.Success(new FileMeta { Name = videoFileName, Path = options.AbsoluteLocalStoragePath });
        }
        public bool TryDelete(string fullFilePath)
        {
            try
            {
                if (File.Exists(fullFilePath))
                {
                    File.Delete(fullFilePath);
                    return !File.Exists(fullFilePath);
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to delete a video file from local storage at path " + fullFilePath);
            }
            return false;
        }
    }
}
