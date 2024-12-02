using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using MicroTube.Data.Models;
using MicroTube.Services.ContentStorage;
using MicroTube.Services.VideoContent.Preprocessing;
using MicroTube.Services.VideoContent.Preprocessing.Stages;
using MicroTube.Services.VideoContent.Processing.Stages;
using MicroTube.Services.Videos;
using MicroTube.Tests.Mock;
using MicroTube.Tests.Utils;
using NSubstitute;
using System.IO.Abstractions.TestingHelpers;

namespace MicroTube.Tests.Unit.VideoContent.Preprocessing
{
	public class CreateUploadProgressStageTests
	{
		[Fact]
		public async Task Execute_Success()
		{
			string generatedName = "somerandomname.mp4";
			string generatedLocationName = "somerandomname";
			MockDbContext mockDb = Database.CreateSqliteInMemoryMock();
			AppUser user = new AppUser {Id = Guid.NewGuid(), Email = "", IsEmailConfirmed = true, PublicUsername = "", Username = "" };
			mockDb.Add(user);
			mockDb.SaveChanges();
			MockFileSystem mockFileSystem = new MockFileSystem();
			var mockRemoteStorage = Substitute.For<IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions>>();
			var meta = new Dictionary<string, string?>();
			mockRemoteStorage.GetLocationMetadata(generatedLocationName).Returns(meta);
			var metaHandler = Substitute.For<IRemoteStorageVideoMetaHandler>();
			metaHandler.ReadDescription(meta).Returns("Description");
			metaHandler.ReadUploaderId(meta).Returns(user.Id.ToString());
			metaHandler.ReadTitle(meta).Returns("Title");
			var mockFormFile = Substitute.For<IFormFile>();
			var stage = new CreateUploadProgressStage(mockFileSystem, mockDb, mockRemoteStorage, metaHandler);
			var context = new DefaultVideoPreprocessingContext
			{
				PreprocessingData = new VideoPreprocessingData(user.Id.ToString(), generatedName, generatedLocationName),
				RemoteCache = new VideoProcessingRemoteCache { VideoFileLocation = generatedLocationName, VideoFileName = generatedName }
			};
			context = await stage.Execute(context);

			var createdProgress = mockDb.VideoUploadProgresses.FirstOrDefault(_ => _.SourceFileRemoteCacheLocation == context.RemoteCache!.VideoFileLocation);
			Assert.NotNull(createdProgress);
			Assert.NotNull(context.UploadProgress);
			Assert.Equal(user.Id, createdProgress.UploaderId);
			Assert.Equal("Title", createdProgress.Title);
			Assert.Equal("Description", createdProgress.Description);
			Assert.Equal(context.RemoteCache!.VideoFileName, createdProgress.SourceFileRemoteCacheFileName);
			Assert.Equal(context.RemoteCache!.VideoFileLocation, createdProgress.SourceFileRemoteCacheLocation);
		}
	}
}
