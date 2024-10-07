using AutoFixture;
using AutoFixture.Kernel;
using MicroTube.Data.Models;
using MicroTube.Services.Base;
using MicroTube.Services.VideoContent.Processing.Stages;
using MicroTube.Tests.Mock.Models;
using MicroTube.Tests.Utils;

namespace MicroTube.Tests.Unit.VideoContent.Processing
{
	public class CreateVideoInDatabaseStageTests
	{
		[Fact]
		public async Task Execute_Success()
		{
			var db = Database.CreateSqliteInMemoryMock();
			var fixture = new Fixture();
			var user = fixture.Build<AppUser>().Without(_=>_.Authentication).Create();
			var uploadProgress = fixture.Build<VideoUploadProgress>().Without(_=>_.Uploader).Create();
			var context = fixture.Build<DefaultVideoProcessingContext>()
				.Without(_ => _.CreatedVideo)
				.With(_ => _.UploadProgress, uploadProgress)
				.Create();
			uploadProgress.Status = VideoUploadStatus.InProgress;
			uploadProgress.Uploader = user;
			db.AddRange(user, uploadProgress);
			db.SaveChanges();

			var stage = new CreateVideoInDatabaseStage(db);
			context = await stage.Execute(context);
			Assert.Equal(PipelineStageState.Finished, stage.State);
			Assert.NotNull(context.CreatedVideo);
			var videoFromDb = db.Videos.FirstOrDefault(_ => _.UploaderId == user.Id);
			Assert.NotNull(videoFromDb);
			Assert.True(videoFromDb.IsEqualByContentValues(context.CreatedVideo));
		}
	}
}
