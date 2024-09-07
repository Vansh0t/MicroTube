using AutoFixture;
using Microsoft.Extensions.Configuration;
using MicroTube.Services;
using MicroTube.Services.Base;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.VideoContent.Processing;
using MicroTube.Services.VideoContent.Processing.Stages;
using NSubstitute;

namespace MicroTube.Tests.Unit.VideoContent.Processing
{
	public class FFMpegCreateQualityTiersStageTests
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
			var config = new ConfigurationBuilder().AddConfigObject(VideoProcessingOptions.KEY, options).Build();
			var compressionServiceMock = Substitute.For<IVideoCompressionService>();
			var tierPaths = fixture.CreateMany<KeyValuePair<int, string>>().ToDictionary(_ => _.Key, _ => _.Value);
			compressionServiceMock
				.CompressToQualityTiers(Arg.Is<IEnumerable<int>>(_=>_.SequenceEqual(options.QualityTiers)), localCache.SourcePath, localCache.QualityTiersLocation)
				.Returns(ServiceResult<IDictionary<int, string>>.Success(tierPaths));
			var stage = new FFMpegCreateQualityTiersStage(compressionServiceMock, config);
			context = await stage.Execute(context);
			Assert.Equal(PipelineStageState.Finished, stage.State);
			Assert.Equal(tierPaths, context.LocalCache!.QualityTierPaths);
		}
	}
}
