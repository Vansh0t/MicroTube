using Microsoft.AspNetCore.Http;
using MicroTube.Data.Models;
using MicroTube.Services.VideoContent.Preprocessing;
using MicroTube.Services.VideoContent.Preprocessing.Stages;
using MicroTube.Services.VideoContent.Processing.Stages;
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
			MockDbContext mockDb = Database.CreateSqliteInMemoryMock();
			AppUser user = new AppUser { Email = "", IsEmailConfirmed = true, PublicUsername = "", Username = "" };
			mockDb.Add(user);
			mockDb.SaveChanges();
			MockFileSystem mockFileSystem = new MockFileSystem();
			var mockFormFile = Substitute.For<IFormFile>();
			var stage = new CreateUploadProgressStage(mockFileSystem, mockDb);
			var context = new DefaultVideoPreprocessingContext
			{
				PreprocessingData = new VideoPreprocessingData(user.Id.ToString(), "video", "description", mockFormFile),
				RemoteCache = new VideoProcessingRemoteCache { VideoFileLocation = "video_location", VideoFileName = "video.mp4"}
			};
			context = await stage.Execute(context);

			var createdProgress = mockDb.VideoUploadProgresses.FirstOrDefault(_ => _.SourceFileRemoteCacheLocation == context.RemoteCache!.VideoFileLocation);
			Assert.NotNull(createdProgress);
			Assert.NotNull(context.UploadProgress);
			Assert.Equal(user.Id, createdProgress.UploaderId);
			Assert.Equal(context.PreprocessingData.VideoTitle, createdProgress.Title);
			Assert.Equal(context.PreprocessingData.VideoDescription, createdProgress.Description);
			Assert.Equal(context.RemoteCache!.VideoFileName, createdProgress.SourceFileRemoteCacheFileName);
			Assert.Equal(context.RemoteCache!.VideoFileLocation, createdProgress.SourceFileRemoteCacheLocation);
		}
	}
}
