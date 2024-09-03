using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicroTube.Services.MediaContentStorage;
using NSubstitute;
using System.Diagnostics;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace MicroTube.Tests.Integration.ContentStorage
{
	public class AzureBlobVideoContentRemoteStorageTests:IDisposable
	{
		private readonly IConfiguration _config;
		private readonly BlobServiceClient _client;
		private readonly ILogger<AzureBlobVideoContentRemoteStorage> _logger;
		private readonly IFileSystem _fileSystem;
		private readonly Process _azuriteProcess;
		public AzureBlobVideoContentRemoteStorageTests()
		{
			_config = new ConfigurationBuilder()
				.AddUserSecrets<AzureBlobVideoContentRemoteStorageTests>()
				.Build();
			_client = new BlobServiceClient(_config["AzureTestStorage:ConnectionString"]);
			_logger = Substitute.For<ILogger<AzureBlobVideoContentRemoteStorage>>();
			_fileSystem = new FileSystem();
			_azuriteProcess = Process.Start("azurite", "--inMemoryPersistence");
		}

		public void Dispose()
		{
			_azuriteProcess.Kill();
		}
		[Fact]
		public async Task DeleteLocation_Success()
		{
			string locationName = Guid.NewGuid().ToString();
			var azureAccess = new AzureBlobVideoContentRemoteStorage(_client, _logger, _fileSystem);
			var containerResult = await _client.CreateBlobContainerAsync(locationName);
			Assert.False(containerResult.GetRawResponse().IsError);
			await azureAccess.DeleteLocation(locationName);
			var blob = _client.GetBlobContainerClient(locationName);
			Assert.False(blob.Exists());
		}
		[Fact]
		public async Task EnsureLocation_Success()
		{
			string locationName = Guid.NewGuid().ToString();
			var azureAccess = new AzureBlobVideoContentRemoteStorage(_client, _logger, _fileSystem);
			await azureAccess.EnsureLocation(locationName, RemoteLocationAccess.Public);
			var blob = _client.GetBlobContainerClient(locationName);
			Assert.True(blob.Exists());
		}
		[Fact]
		public async Task EnsureLocation_ResponseFail()
		{
			string locationName = Guid.NewGuid().ToString();
			var mockClient = Substitute.For<BlobServiceClient>();
			var mockBlobContainerClient = Substitute.For<BlobContainerClient>();
			mockClient.GetBlobContainerClient(locationName).Returns(mockBlobContainerClient);
			var mockResponse = Substitute.For<Azure.Response<BlobContainerInfo>>();
			var mockRawResponse = Substitute.For<Azure.Response>();
			mockResponse.GetRawResponse().Returns(mockRawResponse);
			mockRawResponse.IsError.Returns(true);
			mockBlobContainerClient.CreateIfNotExistsAsync(Arg.Any<PublicAccessType>()).ReturnsForAnyArgs(mockResponse);
			var azureAccess = new AzureBlobVideoContentRemoteStorage(mockClient, _logger, _fileSystem);
			await Assert.ThrowsAnyAsync<ExternalServiceException>(()=> azureAccess.EnsureLocation(locationName, RemoteLocationAccess.Public));
			var blob = _client.GetBlobContainerClient(locationName);
			Assert.False(blob.Exists());
		}
		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		public async Task EnsureLocation_InvalidLocationNameFail(string? locationName)
		{
			var azureAccess = new AzureBlobVideoContentRemoteStorage(_client, _logger, _fileSystem);
			await Assert.ThrowsAnyAsync<Exception>(()=> azureAccess.EnsureLocation(locationName!, RemoteLocationAccess.Public));
		}
		[Fact]
		public async Task UploadFromStream_Success()
		{
			string locationName = Guid.NewGuid().ToString();
			var azureAccess = new AzureBlobVideoContentRemoteStorage(_client, _logger, _fileSystem);
			var path = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TEST_VIDEO_20S_480P_24FPS_LOCATION);
			string fileName = Path.GetFileName(path);
			using var stream = new FileStream(path, FileMode.Open);
			var accessOptions = new AzureBlobAccessOptions(fileName, locationName);
			var uploadOptions = new BlobUploadOptions { AccessTier = AccessTier.Hot };
			var containerResult = await _client.CreateBlobContainerAsync(locationName);
			Assert.False(containerResult.GetRawResponse().IsError);
			var uploadResult = await azureAccess.Upload(stream, accessOptions, uploadOptions);
			Assert.Equal(fileName, uploadResult);
			Assert.True(_client.GetBlobContainerClient(locationName).GetBlobClient(fileName).Exists());
		}
		[Fact]
		public async Task UploadFromFilePath_Success()
		{
			string locationName = Guid.NewGuid().ToString();
			var azureAccess = new AzureBlobVideoContentRemoteStorage(_client, _logger, _fileSystem);
			var path = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TEST_VIDEO_20S_480P_24FPS_LOCATION);
			string fileName = Path.GetFileName(path);
			var accessOptions = new AzureBlobAccessOptions(fileName, locationName);
			var uploadOptions = new BlobUploadOptions { AccessTier = AccessTier.Hot };
			var containerResult =  await _client.CreateBlobContainerAsync(locationName);
			Assert.False(containerResult.GetRawResponse().IsError);
			var uploadResult = await azureAccess.Upload(path, accessOptions, uploadOptions);
			Assert.Equal(fileName, uploadResult);
			Assert.True(_client.GetBlobContainerClient(locationName).GetBlobClient(fileName).Exists());
		}
		[Fact]
		public async Task UploadFromFilePath_NotFileFail()
		{
			string locationName = Guid.NewGuid().ToString();
			var azureAccess = new AzureBlobVideoContentRemoteStorage(_client, _logger, _fileSystem);
			var path = Path.Join(AppDomain.CurrentDomain.BaseDirectory, Constants.TEST_VIDEO_20S_480P_24FPS_LOCATION);
			string fileName = Path.GetFileName(path);
			var accessOptions = new AzureBlobAccessOptions(fileName, locationName);
			var uploadOptions = new BlobUploadOptions { AccessTier = AccessTier.Hot };
			var containerResult = await _client.CreateBlobContainerAsync(locationName);
			Assert.False(containerResult.GetRawResponse().IsError);
			await Assert.ThrowsAnyAsync<ArgumentException>(()=>azureAccess.Upload(Path.GetDirectoryName(path)!, accessOptions, uploadOptions));
			Assert.False(_client.GetBlobContainerClient(locationName).GetBlobClient(fileName).Exists());
		}
		[Theory]
		[InlineData(null, "file.mp4", "real")]
		[InlineData("", "file.mp4", "real")]
		[InlineData(" ", "file.mp4", "real")]
		[InlineData("/valid/path/file.mp4", null, "real")]
		[InlineData("/valid/path/file.mp4", "", "real")]
		[InlineData("/valid/path/file.mp4", " ", "real")]
		[InlineData("/valid/path/file.mp4", "file.mp4", null)]
		[InlineData("/valid/path/file.mp4", "file.mp4", "")]
		[InlineData("/valid/path/file.mp4", "file.mp4", " ")]
		public async Task UploadFromFilePath_InvalidArgumentFail(string? path, string? accessFileName, string? accessLocationName)
		{
			string locationName = Guid.NewGuid().ToString();
			accessLocationName = accessLocationName == "real" ? locationName : accessLocationName;
			MockFileSystem mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
			{
				{"/valid/path/file.mp4", new MockFileData("some_file") }
			});
			var azureAccess = new AzureBlobVideoContentRemoteStorage(_client, _logger, mockFileSystem);
			var accessOptions = new AzureBlobAccessOptions(accessFileName!, accessLocationName!);
			var uploadOptions = new BlobUploadOptions { AccessTier = AccessTier.Hot };
			var containerResult = await _client.CreateBlobContainerAsync(locationName);
			Assert.False(containerResult.GetRawResponse().IsError);
			await Assert.ThrowsAnyAsync<ArgumentException>(() => azureAccess.Upload(path!, accessOptions, uploadOptions));
			Assert.Empty(_client.GetBlobContainerClient(locationName).GetBlobs());
		}
		[Fact]
		public async Task UploadFromFilePath_PathDoesNotExistFail()
		{
			string locationName = Guid.NewGuid().ToString();
			MockFileSystem mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
			{
				{"/valid/path/file.mp4", new MockFileData("some_file") }
			});
			var azureAccess = new AzureBlobVideoContentRemoteStorage(_client, _logger, mockFileSystem);
			var accessOptions = new AzureBlobAccessOptions("file.mp4", locationName);
			var uploadOptions = new BlobUploadOptions { AccessTier = AccessTier.Hot };
			var containerResult = await _client.CreateBlobContainerAsync(locationName);
			Assert.False(containerResult.GetRawResponse().IsError);
			await Assert.ThrowsAnyAsync<IOException>(() => azureAccess.Upload("/invalid/path", accessOptions, uploadOptions));
			Assert.Empty(_client.GetBlobContainerClient(locationName).GetBlobs());
		}
		[Fact]
		public async Task UploadDirectory_Success()
		{
			string locationName = Guid.NewGuid().ToString();
			MockFileSystem mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
			{
				{"/valid/path/file1.mp4", new MockFileData("some_file1") },
				{"/valid/path/file2.mp4", new MockFileData("some_file2") },
				{"/valid/path/file3.mp4", new MockFileData("some_file3") },
				{"/valid/path/file4.mp4", new MockFileData("some_file4") }
			});
			var azureAccess = new AzureBlobVideoContentRemoteStorage(_client, _logger, mockFileSystem);
			var accessOptions = new AzureBlobAccessOptions("", locationName);
			var uploadOptions = new BlobUploadOptions { AccessTier = AccessTier.Hot };
			var containerResult = await _client.CreateBlobContainerAsync(locationName);
			Assert.False(containerResult.GetRawResponse().IsError);
			var result = await azureAccess.UploadDirectory("/valid/path/", accessOptions, uploadOptions);
			Assert.Empty(result.FailedUploadsFileNames);
			Assert.Equal(4, result.SuccessfulUploadsFileNames.Count());
			Assert.Equal(4, _client.GetBlobContainerClient(locationName).GetBlobs().Count());
		}
		[Fact]
		public async Task UploadDirectory_NotDirectoryFail()
		{
			string locationName = Guid.NewGuid().ToString();
			MockFileSystem mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
			{
				{"/valid/path/file1.mp4", new MockFileData("some_file1") }
			});
			var azureAccess = new AzureBlobVideoContentRemoteStorage(_client, _logger, mockFileSystem);
			var accessOptions = new AzureBlobAccessOptions("", locationName);
			var uploadOptions = new BlobUploadOptions { AccessTier = AccessTier.Hot };
			var containerResult = await _client.CreateBlobContainerAsync(locationName);
			Assert.False(containerResult.GetRawResponse().IsError);
			await Assert.ThrowsAnyAsync<ArgumentException>(()=>azureAccess.UploadDirectory("/valid/path/file1.mp4", accessOptions, uploadOptions));
			Assert.Empty(_client.GetBlobContainerClient(locationName).GetBlobs());
		}
		[Fact]
		public async Task Download_Success()
		{
			string locationName = Guid.NewGuid().ToString();
			var mockFileSystem = new MockFileSystem(
				new Dictionary<string, MockFileData> { { "upload.mp4", new MockFileData("some data") } });
			var azureAccess = new AzureBlobVideoContentRemoteStorage(_client, _logger, mockFileSystem);
			using var fileStream = mockFileSystem.FileStream.New("upload.mp4", FileMode.Open);
			await _client.GetBlobContainerClient(locationName).CreateIfNotExistsAsync();
			var uploadResult = await _client.GetBlobContainerClient(locationName).UploadBlobAsync("download.mp4", fileStream);
			Assert.False(uploadResult.GetRawResponse().IsError);
			var downloadResult = await azureAccess.Download("/temp/", new AzureBlobAccessOptions("download.mp4", locationName));
			Assert.True(mockFileSystem.File.Exists("/temp/download.mp4"));
		}
		[Fact]
		public async Task Delete_Success()
		{
			string locationName = Guid.NewGuid().ToString();
			var mockFileSystem = new MockFileSystem(
				new Dictionary<string, MockFileData> { { "upload.mp4", new MockFileData("some data") } });
			var azureAccess = new AzureBlobVideoContentRemoteStorage(_client, _logger, mockFileSystem);
			using var fileStream = mockFileSystem.FileStream.New("upload.mp4", FileMode.Open);
			await _client.GetBlobContainerClient(locationName).CreateIfNotExistsAsync();
			var uploadResult = await _client.GetBlobContainerClient(locationName).UploadBlobAsync("delete.mp4", fileStream);
			Assert.False(uploadResult.GetRawResponse().IsError);
			await azureAccess.Delete(new AzureBlobAccessOptions("delete.mp4", locationName));
			Assert.False(_client.GetBlobContainerClient(locationName).GetBlobClient("delete.mp4").Exists());
		}
	}
}
