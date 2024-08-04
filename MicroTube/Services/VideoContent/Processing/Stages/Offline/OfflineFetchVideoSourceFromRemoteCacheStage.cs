using MicroTube.Services.ConfigOptions;
using MicroTube.Services.MediaContentStorage;
using System.Reflection.Metadata.Ecma335;
using System.Security;

namespace MicroTube.Services.VideoContent.Processing.Stages.Offline
{
    public class OfflineFetchVideoSourceFromRemoteCacheStage : VideoProcessingStage
    {
		private const string THUMBNAILS_SUBLOCATION = "thumbs";
		private const string QUALITY_TIERS_SUBLOCATION = "tiers";
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
			if (context == null)
			{
				throw new ArgumentNullException($"Context must not be null for stage {nameof(OfflineFetchVideoSourceFromRemoteCacheStage)}");
			}
			if (context.RemoteCache == null)
			{
				throw new ArgumentNullException($"{nameof(context.RemoteCache)} must not be null for stage {nameof(OfflineFetchVideoSourceFromRemoteCacheStage)}");
			}
			VideoProcessingOptions options = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
			string workingLocation = CreateWorkingLocation(options.AbsoluteLocalStoragePath, context.SourceVideoNameWithoutExtension!);
			string downloadedPath = await DownloadSourceFromRemoteCache(
				context.RemoteCache.VideoFileName,
				context.RemoteCache.VideoFileLocation,
				workingLocation,
				cancellationToken);
            string localCacheFileName = Path.GetFileName(downloadedPath);
            string? localCacheFileLocation = Path.GetDirectoryName(downloadedPath);
			if (string.IsNullOrWhiteSpace(localCacheFileLocation))
			{
				throw new SecurityException($"A video source file was downloaded in invalid location {downloadedPath}");
			}
			context.LocalCache = new VideoProcessingLocalCache
            {
                SourceLocation = localCacheFileLocation,
                SourceFileName = localCacheFileName,
				WorkingLocation = workingLocation,
				ThumbnailsLocation = CreateThumbnailsLocation(workingLocation),
				QualityTiersLocation = CreateQualityTiersLocation(workingLocation)
			};
            return context;
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
		private string CreateWorkingLocation(string path, string normalizedFileName)
		{
			string newLocation = Path.Join(path, normalizedFileName);
			Directory.CreateDirectory(newLocation);
			return newLocation;
		}
		private string CreateThumbnailsLocation(string workingLocation)
		{
			string thumbnailsLocation = Path.Join(workingLocation, THUMBNAILS_SUBLOCATION);
			Directory.CreateDirectory(thumbnailsLocation);
			return thumbnailsLocation;
		}
		private string CreateQualityTiersLocation(string workingLocation)
		{
			string thumbnailsLocation = Path.Join(workingLocation, QUALITY_TIERS_SUBLOCATION);
			Directory.CreateDirectory(thumbnailsLocation);
			return thumbnailsLocation;
		}
	}
}
