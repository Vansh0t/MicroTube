using Microsoft.AspNetCore.Http;
using MicroTube.Services.VideoContent.Preprocessing.Stages;
using MicroTube.Services.VideoContent.Preprocessing;
using NSubstitute;
using System.IO.Abstractions.TestingHelpers;
using MicroTube.Services.VideoContent;
using MicroTube.Services.ContentStorage;
using Azure.Storage.Blobs.Models;

namespace MicroTube.Tests.Unit.VideoContent.Preprocessing
{
	public class UploadVideoSourceToRemoteStorageStageTests
	{
		//[Fact]
		//public async Task Execute_Success()
		//{
		//	var mockFormFile = Substitute.For<IFormFile>();
		//	var mockFileStream = Substitute.For<Stream>();
		//	mockFormFile.OpenReadStream().Returns(mockFileStream);
		//	var context = new DefaultVideoPreprocessingContext
		//	{
		//		PreprocessingData = new VideoPreprocessingData("id", "generatedname.mp4", "generatedname")
		//	};
		//	var mockFileSystem = new MockFileSystem();
		//	var mockVideoNameGenerator = Substitute.For<IVideoFileNameGenerator>();
		//	mockVideoNameGenerator.GenerateVideoName().Returns("vid.mp4");
		//	var mockLocationNameGenerator = Substitute.For<IRemoteLocationNameGenerator>();
		//	mockLocationNameGenerator.GetLocationName("vid.mp4").Returns("vid");
		//	var mockRemoteStorage = Substitute.For<IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions>>();
		//	mockRemoteStorage.Upload(mockFileStream, Arg.Any<AzureBlobAccessOptions>(), Arg.Any<BlobUploadOptions>()).Returns("vid.mp4");
		//	var stage = new EnsureVideoSourceInRemoteStorage(mockVideoNameGenerator, mockFileSystem, mockLocationNameGenerator, mockRemoteStorage);
		//
		//	context = await stage.Execute(context);
		//	Assert.NotNull(context.RemoteCache);
		//	Assert.Equal("vid", context.RemoteCache.VideoFileLocation);
		//	Assert.Equal("vid.mp4", context.RemoteCache.VideoFileName);
		//}
	}
}
