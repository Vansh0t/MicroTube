using MicroTube.Services.ConfigOptions;
using MicroTube.Services.MediaContentStorage;

namespace MicroTube.Services.VideoContent.Processing.Stages.Offline
{
    public class OfflineFetchVideoSourceFromRemoteCacheStage : VideoProcessingStage
    {
        private readonly IConfiguration _config;
        private readonly IVideoContentRemoteStorage<OfflineRemoteStorageOptions, OfflineRemoteStorageOptions> _remoteStorage;
		public OfflineFetchVideoSourceFromRemoteCacheStage(
			IVideoContentRemoteStorage<OfflineRemoteStorageOptions, OfflineRemoteStorageOptions> remoteStorage, IConfiguration config)
		{
			_remoteStorage = remoteStorage;
			_config = config;
		}
		protected override async Task<DefaultVideoProcessingContext> ExecuteInternal(DefaultVideoProcessingContext? context, CancellationToken cancellationToken)
        {
            ValidateContext(context);
			VideoProcessingOptions options = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
			string downloadedPath = await DownloadSourceFromRemoteCache(context!.RemoteCache!.VideoFileName!, context.RemoteCache.VideoFileLocation!, options.AbsoluteLocalStoragePath, cancellationToken);
            string localCacheFileName = Path.GetFileName(downloadedPath);
            string? localCacheFileLocation = Path.GetDirectoryName(downloadedPath);
            context.LocalCache = new SourceVideoLocalCache
            {
                VideoFileLocation = localCacheFileLocation,
                VideoFileName = localCacheFileName
            };
            return context;
        }
        private void ValidateContext(DefaultVideoProcessingContext? context)
        {
            if (context == null)
            {
                throw new ArgumentNullException($"Context must not be null for stage {nameof(OfflineFetchVideoSourceFromRemoteCacheStage)}");
            }
            if (context.RemoteCache == null)
            {
                throw new ArgumentNullException($"{nameof(context.RemoteCache)} must not be null for stage {nameof(OfflineFetchVideoSourceFromRemoteCacheStage)}");
            }
            if (string.IsNullOrWhiteSpace(context.RemoteCache.VideoFileName))
            {
                throw new ArgumentNullException($"{context.RemoteCache.VideoFileName} must not be null for stage {nameof(OfflineFetchVideoSourceFromRemoteCacheStage)}");
            }
            if (string.IsNullOrWhiteSpace(context.RemoteCache.VideoFileLocation))
            {
                throw new ArgumentNullException($"{context.RemoteCache.VideoFileLocation} must not be null for stage {nameof(OfflineFetchVideoSourceFromRemoteCacheStage)}");
            }
        }
        private async Task<string> DownloadSourceFromRemoteCache(string sourceFileName, string sourceLocation, string saveToPath, CancellationToken cancellationToken)
        {
            var remoteAccessOptions = new OfflineRemoteStorageOptions(Path.Join(sourceLocation, sourceFileName));
            var remoteCacheDownloadResult = await _remoteStorage.Download(saveToPath, remoteAccessOptions, cancellationToken);
            if (remoteCacheDownloadResult.IsError)
            {
                throw new BackgroundJobException($"Failed to download from remote cache for processing. File: {sourceFileName}, Location: {sourceLocation}, SaveTo: {saveToPath}.");
            }
            return remoteCacheDownloadResult.GetRequiredObject();
        }
    }
}
