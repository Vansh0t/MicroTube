﻿using Ardalis.GuardClauses;
using Azure.Storage.Blobs.Models;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.ContentStorage;
using System.IO.Abstractions;
using System.Security;

namespace MicroTube.Services.VideoContent.Processing.Stages.Azure
{
	public class AzureFetchVideoSourceFromRemoteCacheStage : VideoProcessingStage
	{
		public const string THUMBNAILS_SUBLOCATION = "thumbs";
		public const string QUALITY_TIERS_SUBLOCATION = "tiers";

		private readonly IConfiguration _config;
		private readonly IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> _remoteStorage;
		private readonly IFileSystem _fileSystem;

		public AzureFetchVideoSourceFromRemoteCacheStage(IConfiguration config, IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> remoteStorage, IFileSystem fileSystem)
		{
			_config = config;
			_remoteStorage = remoteStorage;
			_fileSystem = fileSystem;
		}

		protected override async Task<DefaultVideoProcessingContext> ExecuteInternal(DefaultVideoProcessingContext? context, CancellationToken cancellationToken)
		{
			Guard.Against.Null(context);
			Guard.Against.Null(context.RemoteCache);
			VideoProcessingOptions options = _config.GetRequiredByKey<VideoProcessingOptions>(VideoProcessingOptions.KEY);
			string workingLocation = CreateWorkingLocation(options.AbsoluteLocalStoragePath, context.RemoteCache.VideoFileLocation);
			string downloadedPath = await DownloadSourceFromRemoteCache(
				context.RemoteCache.VideoFileName,
				context.RemoteCache.VideoFileLocation,
				workingLocation,
				cancellationToken);
			string localCacheFileName = _fileSystem.Path.GetFileName(downloadedPath);
			string? localCacheFileLocation = _fileSystem.Path.GetDirectoryName(downloadedPath);
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
			var remoteAccessOptions = new AzureBlobAccessOptions(sourceFileName, sourceLocation);
			try
			{
				string downloadPath = await _remoteStorage.Download(saveToPath, remoteAccessOptions, cancellationToken);
				return downloadPath;
			}
			catch (Exception e)
			{
				throw new BackgroundJobException($"Failed to download from remote cache for processing. File: {sourceFileName}, Location: {sourceLocation}, SaveTo: {saveToPath}.", e);
			}
		}
		private string CreateWorkingLocation(string path, string normalizedFileName)
		{
			string newLocation = _fileSystem.Path.Join(path, normalizedFileName);
			_fileSystem.Directory.CreateDirectory(newLocation);
			return newLocation;
		}
		private string CreateThumbnailsLocation(string workingLocation)
		{
			string thumbnailsLocation = _fileSystem.Path.Join(workingLocation, THUMBNAILS_SUBLOCATION);
			_fileSystem.Directory.CreateDirectory(thumbnailsLocation);
			return thumbnailsLocation;
		}
		private string CreateQualityTiersLocation(string workingLocation)
		{
			string thumbnailsLocation = _fileSystem.Path.Join(workingLocation, QUALITY_TIERS_SUBLOCATION);
			_fileSystem.Directory.CreateDirectory(thumbnailsLocation);
			return thumbnailsLocation;
		}
	}
}
