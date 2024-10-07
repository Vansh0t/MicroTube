using AutoFixture;
using Microsoft.Extensions.Configuration;
using MicroTube.Services.Base;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.VideoContent.Processing.Stages;
using MicroTube.Services.VideoContent.Processing;
using MicroTube.Services;

namespace MicroTube.Tests.Unit.VideoContent.Processing
{
	public class FFMpegCreateThumbnailsStageTests
	{
		[Fact]
		public async Task Execute_Success()
		{
			var fixture = new Fixture();
			var localCache = fixture.Create<VideoProcessingLocalCache>();
			var context = fixture.Build<DefaultVideoProcessingContext>()
				.OmitAutoProperties()
				.With(_ => _.LocalCache, localCache)
				.Create();
			var options = fixture.Create<VideoProcessingOptions>();
			options.ThumbnailsQualityTier = context.LocalCache!.QualityTierPaths!.Last().Key;
			var config = new ConfigurationBuilder().AddConfigObject(VideoProcessingOptions.KEY, options).Build();
			var thumbnailsServiceMock = Substitute.For<IVideoThumbnailsService>();
			var tierPaths = fixture.CreateMany<KeyValuePair<int, string>>().ToDictionary(_ => _.Key, _ => _.Value);
			var thumbnailPaths = fixture.CreateMany<string>();
			thumbnailsServiceMock
				.MakeThumbnails(context.LocalCache.QualityTierPaths[options.ThumbnailsQualityTier], localCache.ThumbnailsLocation)
				.Returns(ServiceResult<IEnumerable<string>>.Success(thumbnailPaths));
			var stage = new FFMpegCreateThumbnailsStage(thumbnailsServiceMock, config);
			context = await stage.Execute(context);
			Assert.Equal(PipelineStageState.Finished, stage.State);
		}
	}
}
