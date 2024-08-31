﻿using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using MicroTube.Services;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.MediaContentStorage;
using MicroTube.Services.VideoContent.Processing.Stages;
using MicroTube.Services.VideoContent.Processing.Stages.Azure;
using NSubstitute;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Security;

namespace MicroTube.Tests.Unit.VideoContent.Processing
{
	public class AzureFetchVideoSourceFromRemoteCacheStageTests
	{
		[Fact]
		public async Task ExecuteStage_Success()
		{
			var context = new DefaultVideoProcessingContext()
			{
				SourceVideoNameWithoutExtension = "source",
				RemoteCache = new VideoProcessingRemoteCache { VideoFileName = "source.mp4", VideoFileLocation = "videos" }
			};
			string localStoragePath = "valid/path";
			string workLocation = Path.Join(localStoragePath, context.SourceVideoNameWithoutExtension);
			string expectedLocalCacheSourcePath = "valid/path/source/source.mp4";
			var config = new ConfigurationBuilder()
				.AddConfigObject(VideoProcessingOptions.KEY, new VideoProcessingOptions("", 0, 0, localStoragePath, 0, 0, 0, 0, 0, 0, 0, 0, 0))
				.Build();
			var remoteStorage = Substitute.For<IVideoContentRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions>>();
			remoteStorage.Download(workLocation, Arg.Any<AzureBlobAccessOptions>())
				.Returns(ServiceResult<string>.Success(expectedLocalCacheSourcePath));
			var fileSystem = new MockFileSystem();
			fileSystem.Directory.CreateDirectory(localStoragePath);
			
			var stage = new AzureFetchVideoSourceFromRemoteCacheStage(config, remoteStorage, fileSystem);
			var result = await stage.Execute(context);
			Assert.NotNull(context.LocalCache);
			var norm = Path.GetFullPath(result.LocalCache.ThumbnailsLocation);
			var norm1 = Path.GetFullPath("valid/path/source/thumbs");
			Assert.Equal(Path.GetFullPath("valid/path/source"), Path.GetFullPath(result.LocalCache!.WorkingLocation));
			Assert.Equal(Path.GetFullPath("valid/path/source/thumbs"), Path.GetFullPath(result.LocalCache.ThumbnailsLocation));
			Assert.Equal(Path.GetFullPath("valid/path/source/tiers"), Path.GetFullPath(result.LocalCache.QualityTiersLocation));
			Assert.Equal(Path.GetFullPath("valid/path/source"), Path.GetFullPath(result.LocalCache.SourceLocation));
			Assert.Equal("source.mp4", result.LocalCache.SourceFileName);
			Assert.True(fileSystem.Directory.Exists("valid/path/source"));
			Assert.True(fileSystem.Directory.Exists("valid/path/source/thumbs"));
			Assert.True(fileSystem.Directory.Exists("valid/path/source/tiers"));
		}
		[Fact]
		public async Task ExecuteStage_InvalidContextFail()
		{
			var context = new DefaultVideoProcessingContext()
			{
				SourceVideoNameWithoutExtension = "source",
			};
			string localStoragePath = "valid/path";
			string workLocation = Path.Join(localStoragePath, context.SourceVideoNameWithoutExtension);
			string expectedLocalCacheSourcePath = "valid/path/source/source.mp4";
			var config = new ConfigurationBuilder()
				.AddConfigObject(VideoProcessingOptions.KEY, new VideoProcessingOptions("", 0, 0, localStoragePath, 0, 0, 0, 0, 0, 0, 0, 0, 0))
				.Build();
			var remoteStorage = Substitute.For<IVideoContentRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions>>();
			remoteStorage.Download(workLocation, Arg.Any<AzureBlobAccessOptions>())
				.Returns(ServiceResult<string>.Success(expectedLocalCacheSourcePath));
			var fileSystem = new MockFileSystem();
			fileSystem.Directory.CreateDirectory(localStoragePath);

			var stage = new AzureFetchVideoSourceFromRemoteCacheStage(config, remoteStorage, fileSystem);
			await Assert.ThrowsAnyAsync<ArgumentNullException>(()=> stage.Execute(null));
			await Assert.ThrowsAnyAsync<ArgumentNullException>(()=> stage.Execute(context));
		}
		[Fact]
		public async Task ExecuteStage_RemoteCacheFail()
		{
			var context = new DefaultVideoProcessingContext()
			{
				SourceVideoNameWithoutExtension = "source",
				RemoteCache = new VideoProcessingRemoteCache { VideoFileName = "source.mp4", VideoFileLocation = "videos" }
			};
			string localStoragePath = "valid/path";
			string workLocation = Path.Join(localStoragePath, context.SourceVideoNameWithoutExtension);
			var config = new ConfigurationBuilder()
				.AddConfigObject(VideoProcessingOptions.KEY, new VideoProcessingOptions("", 0, 0, localStoragePath, 0, 0, 0, 0, 0, 0, 0, 0, 0))
				.Build();
			var remoteStorage = Substitute.For<IVideoContentRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions>>();
			remoteStorage.Download(workLocation, Arg.Any<AzureBlobAccessOptions>())
				.Returns(ServiceResult<string>.FailInternal());
			var fileSystem = new MockFileSystem();
			fileSystem.Directory.CreateDirectory(localStoragePath);

			var stage = new AzureFetchVideoSourceFromRemoteCacheStage(config, remoteStorage, fileSystem);
			await Assert.ThrowsAnyAsync<BackgroundJobException>(() => stage.Execute(context));
		}
		[Fact]
		public async Task ExecuteStage_InvalidDownloadPathFail()
		{
			var context = new DefaultVideoProcessingContext()
			{
				RemoteCache = new VideoProcessingRemoteCache { VideoFileName = "source.mp4", VideoFileLocation = "videos" },
				SourceVideoNameWithoutExtension = "source",
			};
			string localStoragePath = "valid/path";
			string workLocation = Path.Join(localStoragePath, context.SourceVideoNameWithoutExtension);
			var config = new ConfigurationBuilder()
				.AddConfigObject(VideoProcessingOptions.KEY, new VideoProcessingOptions("", 0, 0, localStoragePath, 0, 0, 0, 0, 0, 0, 0, 0, 0))
				.Build();
			var remoteStorage = Substitute.For<IVideoContentRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions>>();
			remoteStorage.Download(workLocation, Arg.Any<AzureBlobAccessOptions>())
				.Returns(ServiceResult<string>.Success(""));
			var fileSystem = new MockFileSystem();
			fileSystem.Directory.CreateDirectory(localStoragePath);

			var stage = new AzureFetchVideoSourceFromRemoteCacheStage(config, remoteStorage, fileSystem);
			await Assert.ThrowsAnyAsync<SecurityException>(() => stage.Execute(context));
		}
	}
}