using AutoFixture;
using MicroTube.Data.Models;
using MicroTube.Data.Models.Videos;
using MicroTube.Services.Base;
using MicroTube.Services.VideoContent.Processing.Stages;
using MicroTube.Tests.Utils;

namespace MicroTube.Tests.Unit.VideoContent.Processing
{
    public class FetchVideoUploadProgressStageTests
	{
		[Fact]
		public async Task Execute_Success()
		{
			var db = Database.CreateSqliteInMemoryMock();
			var fixture = new Fixture();
			var user = fixture.Build<AppUser>().Without(_ => _.Authentication).Create();
			var uploadProgress = fixture.Build<VideoUploadProgress>().Without(_ => _.Uploader).Create();
			var context = fixture.Build<DefaultVideoProcessingContext>()
				.Without(_ => _.CreatedVideo)
				.Without(_ => _.LocalCache)
				.Without(_ => _.UploadProgress)
				.Create();
			uploadProgress.SourceFileRemoteCacheFileName = context.RemoteCache!.VideoFileName;
			uploadProgress.SourceFileRemoteCacheLocation = context.RemoteCache!.VideoFileLocation;
			uploadProgress.Status = VideoUploadStatus.InProgress;
			uploadProgress.Uploader = user;
			db.AddRange(user, uploadProgress);
			db.SaveChanges();

			var stage = new FetchVideoUploadProgressStage(db);
			context = await stage.Execute(context);
			Assert.Equal(PipelineStageState.Finished, stage.State);
			Assert.NotNull(context.UploadProgress);
			var progressFromDb = db.VideoUploadProgresses.FirstOrDefault(_ => _.UploaderId == user.Id);
			Assert.NotNull(progressFromDb);
			Assert.True(progressFromDb.IsEqualByContentValues(context.UploadProgress));
		}
	}
}
