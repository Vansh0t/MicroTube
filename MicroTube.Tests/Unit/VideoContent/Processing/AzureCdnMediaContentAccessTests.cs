using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.ContentStorage;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.IO.Abstractions.TestingHelpers;

namespace MicroTube.Tests.Unit.VideoContent.Processing
{
	public class AzureCdnMediaContentAccessTests
	{
		[Fact]
		public async Task UploadVideoQualityTiers_Success()
		{
			string directory = "/some/directory";
			string containerName = "container_name";
			var mockRemoteStorage = Substitute.For<IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions>>();
			var config = new ConfigurationBuilder().AddConfigObject(VideoContentUploadOptions.KEY, new VideoContentUploadOptions("http://cdn.com", 5)).Build();
			var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
				{ "/tiers/file1.mp4", "content" },
				{ "/tiers/file2.mp4", "content" },
				{ "/tiers/file3.mp4", "content" },
				{ "/tiers/file4.mp4", "content" }
			});
			var mockBulkUploadResult = new BulkUploadResult(new List<string> { "file1.mp4", "file2.mp4", "file3.mp4", "file4.mp4" }, Array.Empty<string>());
			mockRemoteStorage.UploadDirectory(directory, Arg.Any<AzureBlobAccessOptions>(), Arg.Any<BlobUploadOptions>()).Returns(mockBulkUploadResult);
			var cdn = new AzureCdnMediaContentAccess(mockRemoteStorage, config, Substitute.For<ILogger<AzureCdnMediaContentAccess>>(), mockFileSystem);
			var result = await cdn.UploadVideoQualityTiers(directory, containerName);
			Assert.False(result.IsError);
			Assert.Equal(200, result.Code);
			var urls = result.GetRequiredObject().ToArray();
			Assert.Equal(4, urls.Length);
			Assert.Equal($"http://cdn.com/{containerName}/file1.mp4", urls[0].ToString());
			Assert.Equal($"http://cdn.com/{containerName}/file2.mp4", urls[1].ToString());
			Assert.Equal($"http://cdn.com/{containerName}/file3.mp4", urls[2].ToString());
			Assert.Equal($"http://cdn.com/{containerName}/file4.mp4", urls[3].ToString());
		}
		[Fact]
		public async Task UploadVideo_PartialSuccess_Returns207()
		{
			string directory = "/some/directory";
			string containerName = "container_name";
			var mockRemoteStorage = Substitute.For<IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions>>();
			var config = new ConfigurationBuilder().AddConfigObject(VideoContentUploadOptions.KEY, new VideoContentUploadOptions("http://cdn.com", 5)).Build();
			var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
				{ "/tiers/file1.mp4", "content" },
				{ "/tiers/file2.mp4", "content" },
				{ "/tiers/file3.mp4", "content" },
				{ "/tiers/file4.mp4", "content" }
			});
			var mockBulkUploadResult = new BulkUploadResult(new List<string> { "file1.mp4", "file2.mp4" }, new List<string> { "file3.mp4", "file4.mp4" });
			mockRemoteStorage.UploadDirectory(directory, Arg.Any<AzureBlobAccessOptions>(), Arg.Any<BlobUploadOptions>()).Returns(mockBulkUploadResult);
			var cdn = new AzureCdnMediaContentAccess(mockRemoteStorage, config, Substitute.For<ILogger<AzureCdnMediaContentAccess>>(), mockFileSystem);
			var result = await cdn.UploadVideoQualityTiers(directory, containerName);
			Assert.False(result.IsError);
			Assert.Equal(207, result.Code);
			var urls = result.GetRequiredObject().ToArray();
			Assert.Equal(2, urls.Length);
			Assert.Equal($"http://cdn.com/{containerName}/file1.mp4", urls[0].ToString());
			Assert.Equal($"http://cdn.com/{containerName}/file2.mp4", urls[1].ToString());
		}
		[Theory]
		[InlineData(null, "container")]
		[InlineData("", "container")]
		[InlineData(" ", "container")]
		[InlineData("file.mp4", null)]
		[InlineData("file.mp4", "")]
		[InlineData("file.mp4", " ")]
		public async Task UploadVideo_InvalidArgumentsFail(string? directoryName, string? containerName)
		{
			var mockRemoteStorage = Substitute.For<IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions>>();
			var config = new ConfigurationBuilder().AddConfigObject(VideoContentUploadOptions.KEY, new VideoContentUploadOptions("http://cdn.com", 5)).Build();
			var mockFileSystem = new MockFileSystem();
			var cdn = new AzureCdnMediaContentAccess(mockRemoteStorage, config, Substitute.For<ILogger<AzureCdnMediaContentAccess>>(), mockFileSystem);
			var result = await cdn.UploadVideoQualityTiers(directoryName!, containerName!);
			Assert.True(result.IsError);
			Assert.Equal(400, result.Code);
		}
		[Fact]
		public async Task UploadVideoThumbnails_Success()
		{
			string directory = "/some/directory";
			string containerName = "container_name";
			var mockRemoteStorage = Substitute.For<IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions>>();
			var config = new ConfigurationBuilder().AddConfigObject(VideoContentUploadOptions.KEY, new VideoContentUploadOptions("http://cdn.com", 5)).Build();
			var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
				{ "/tiers/file1.mp4", "content" },
				{ "/tiers/file2.mp4", "content" },
				{ "/tiers/file3.mp4", "content" },
				{ "/tiers/file4.mp4", "content" }
			});
			var mockBulkUploadResult = new BulkUploadResult(new List<string> { "file1.mp4", "file2.mp4", "file3.mp4", "file4.mp4" }, Array.Empty<string>());
			mockRemoteStorage.UploadDirectory(directory, Arg.Any<AzureBlobAccessOptions>(), Arg.Any<BlobUploadOptions>()).Returns(mockBulkUploadResult);
			var cdn = new AzureCdnMediaContentAccess(mockRemoteStorage, config, Substitute.For<ILogger<AzureCdnMediaContentAccess>>(), mockFileSystem);
			var result = await cdn.UploadVideoThumbnails(directory, containerName);
			Assert.False(result.IsError);
			Assert.Equal(200, result.Code);
			var urls = result.GetRequiredObject().ToArray();
			Assert.Equal(4, urls.Length);
			Assert.Equal($"http://cdn.com/{containerName}/file1.mp4", urls[0].ToString());
			Assert.Equal($"http://cdn.com/{containerName}/file2.mp4", urls[1].ToString());
			Assert.Equal($"http://cdn.com/{containerName}/file3.mp4", urls[2].ToString());
			Assert.Equal($"http://cdn.com/{containerName}/file4.mp4", urls[3].ToString());
		}
	}
}
