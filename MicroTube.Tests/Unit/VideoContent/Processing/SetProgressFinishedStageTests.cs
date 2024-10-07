using AutoFixture;
using MicroTube.Data.Models;
using MicroTube.Services.Base;
using MicroTube.Services.VideoContent.Processing;
using MicroTube.Services.VideoContent.Processing.Stages;
using MicroTube.Tests.Utils;

namespace MicroTube.Tests.Unit.VideoContent.Processing
{
	public class SetProgressFinishedStageTests
	{
		[Fact]
		public async Task Execute_Success()
		{
			var fixture = new Fixture();
			var db = Database.CreateSqliteInMemoryMock();
			var user = fixture.Build<AppUser>().Without(_ => _.Authentication).Create();
			var videoProgress = fixture.Build<VideoUploadProgress>().Without(_=>_.Uploader).With(_ => _.UploaderId, user.Id).Create();
			db.AddRange(user, videoProgress);
			db.SaveChanges();
			var context = fixture.Build<DefaultVideoProcessingContext>().OmitAutoProperties().With(_=>_.UploadProgress, videoProgress).Create();
			var videoAnalyzerMock = Substitute.For<IVideoAnalyzer>();
			var analyzeResult = fixture.Create<VideoFileMetaData>();
			videoAnalyzerMock.Analyze("").ReturnsForAnyArgs(analyzeResult);
			var stage = new SetProgressFinishedStage(videoAnalyzerMock, db);
			context = await stage.Execute(context);
			Assert.Equal(PipelineStageState.Finished, stage.State);
			Assert.Equal(VideoUploadStatus.Success, context.UploadProgress!.Status);
			var progressFromDb = db.VideoUploadProgresses.FirstOrDefault(_ => _.Id == videoProgress.Id);
			Assert.NotNull(progressFromDb);
			Assert.Equal(VideoUploadStatus.Success, progressFromDb.Status);
		}
	}
}
