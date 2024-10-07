using Hangfire;
using Hangfire.Annotations;
using Hangfire.Common;
using Hangfire.States;
using MicroTube.Services.VideoContent.Preprocessing;
using MicroTube.Services.VideoContent.Preprocessing.Stages;
using MicroTube.Services.VideoContent.Processing;
using MicroTube.Services.VideoContent.Processing.Stages;
using MicroTube.Tests.Mock;
using NSubstitute;
using System;
using System.Linq.Expressions;

namespace MicroTube.Tests.Unit.VideoContent.Preprocessing
{
	public class MakeProcessingJobStageTests
	{
		[Fact]
		public async Task Execute()
		{
			var context = new DefaultVideoPreprocessingContext()
			{
				PreprocessingData = new VideoPreprocessingData("id", "vid_title", "desc", null!),
				RemoteCache = new VideoProcessingRemoteCache { VideoFileLocation = "video", VideoFileName = "video.mp4" }
			};
			Expression<Func<IVideoProcessingPipeline, Task>> job = processing => processing.Execute(new DefaultVideoProcessingContext(), default); //TO DO: check if arguments passed into job correctly
			var testJobClient = new TestBackgroundJobClient();
			var stage = new MakeProcessingJobStage(testJobClient);
			await stage.Execute(context);
			Assert.NotNull(testJobClient.CreatedJob);
			DefaultVideoProcessingContext? argContext = testJobClient.CreatedJob.Args.FirstOrDefault() as DefaultVideoProcessingContext;
			Assert.NotNull(argContext);
			Assert.Equal(context.RemoteCache, argContext.RemoteCache);
		}
	}
}
