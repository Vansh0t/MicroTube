using MicroTube.Services.MediaContentStorage;
using MicroTube.Services.VideoContent.Processing.Stages.Azure;
using MicroTube.Services.VideoContent.Processing.Stages;
using MicroTube.Services;
using NSubstitute;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Abstractions;

namespace MicroTube.Tests.Unit.VideoContent.Processing
{
	public class AzureUploadVideoToCdnStageTests
	{
		private readonly ICdnMediaContentAccess _cdnMock;
		private readonly IFileSystem _fileSystemMock;
		private readonly IList<Uri> uploadedUrls;
		public AzureUploadVideoToCdnStageTests()
		{
			_cdnMock = Substitute.For<ICdnMediaContentAccess>();
			_fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
			{
				{"valid/path/source/tiers/source_144.mp4", new MockFileData ("data") },
				{"valid/path/source/tiers/source_240.mp4", new MockFileData ("data") },
				{"valid/path/source/tiers/source_360.mp4", new MockFileData ("data") },
				{"valid/path/source/tiers/source_480.mp4", new MockFileData ("data") },
				{"valid/path/source/tiers/source_720.mp4", new MockFileData ("data") },
			});
			uploadedUrls = new List<Uri>
			{
				new Uri("https://cdn.test/source_144.mp4"),
				new Uri("https://cdn.test/source_240.mp4"),
				new Uri("https://cdn.test/source_360.mp4"),
				new Uri("https://cdn.test/source_480.mp4"),
				new Uri("https://cdn.test/source_720.mp4")
			};
			_cdnMock.UploadVideo(Arg.Any<Stream>(), "source_144.mp4", "").Returns(ServiceResult<Uri>.Success(uploadedUrls[0]));
			_cdnMock.UploadVideo(Arg.Any<Stream>(), "source_240.mp4", "").Returns(ServiceResult<Uri>.Success(uploadedUrls[1]));
			_cdnMock.UploadVideo(Arg.Any<Stream>(), "source_360.mp4", "").Returns(ServiceResult<Uri>.Success(uploadedUrls[2]));
			_cdnMock.UploadVideo(Arg.Any<Stream>(), "source_480.mp4", "").Returns(ServiceResult<Uri>.Success(uploadedUrls[3]));
			_cdnMock.UploadVideo(Arg.Any<Stream>(), "source_720.mp4", "").Returns(ServiceResult<Uri>.Success(uploadedUrls[4]));
		}
		[Fact]
		public async Task ExecuteStage_Success()
		{
			var context = new DefaultVideoProcessingContext()
			{
				SourceVideoNameWithoutExtension = "source",
				LocalCache = new VideoProcessingLocalCache
				{
					QualityTiersLocation = "valid/path/source/tiers",
					SourceLocation = "valid/path/source",
					SourceFileName = "source.mp4",
					ThumbnailsLocation = "valid/path/source/thumbs",
					WorkingLocation = "valid/path/source"
				}
			};
			var stage = new AzureUploadVideoToCdnStage(_cdnMock, _fileSystemMock);
			var result = await stage.Execute(context);
			Assert.NotNull(result.Cdn);
			Assert.NotNull(result.Cdn.VideoEndpoints);
			var endpoints = result.Cdn.VideoEndpoints.ToArray();
			Assert.Equal(uploadedUrls.Count, endpoints.Count());
			Assert.Equal(endpoints[0], uploadedUrls[0]);
			Assert.Equal(endpoints[1], uploadedUrls[1]);
			Assert.Equal(endpoints[2], uploadedUrls[2]);
			Assert.Equal(endpoints[3], uploadedUrls[3]);
			Assert.Equal(endpoints[4], uploadedUrls[4]);
		}
		[Fact]
		public async Task ExecuteStage_InvalidContextFail()
		{
			var context = new DefaultVideoProcessingContext()
			{
				SourceVideoNameWithoutExtension = "source",
			};
			var stage = new AzureUploadVideoToCdnStage(_cdnMock, _fileSystemMock);
			await Assert.ThrowsAnyAsync<ArgumentNullException>(()=> stage.Execute(null));
			await Assert.ThrowsAnyAsync<ArgumentNullException>(()=> stage.Execute(context));
		}
		[Fact]
		public async Task ExecuteStage_CdnUploadFail()
		{
			var context = new DefaultVideoProcessingContext()
			{
				SourceVideoNameWithoutExtension = "source",
				LocalCache = new VideoProcessingLocalCache
				{
					QualityTiersLocation = "valid/path/source/tiers",
					SourceLocation = "valid/path/source",
					SourceFileName = "source.mp4",
					ThumbnailsLocation = "valid/path/source/thumbs",
					WorkingLocation = "valid/path/source"
				}
			};
			ICdnMediaContentAccess failedCdn = Substitute.For<ICdnMediaContentAccess>();
			failedCdn.UploadVideo(Arg.Any<Stream>(), Arg.Any<string>(), "").Returns(ServiceResult<Uri>.FailInternal());
			var stage = new AzureUploadVideoToCdnStage(failedCdn, _fileSystemMock);
			await Assert.ThrowsAnyAsync<BackgroundJobException>(() => stage.Execute(context));
		}
	}
}
