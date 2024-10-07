using Microsoft.AspNetCore.Mvc.Testing;
using MicroTube.Tests.Mock;

namespace MicroTube.Tests.Integration.Startup
{
	public class StartupTests: IClassFixture<StartupTestWebAppFactory>
	{
		private readonly StartupTestWebAppFactory _factory;

		public StartupTests(StartupTestWebAppFactory factory)
		{
			_factory = factory;
		}
		//[Fact(Skip = "Temporary disable to prevent CI fails due to invalid secrets config.")]
		public async Task RunWebApp_Success()
		{
			var client = _factory.CreateClient();
		}
	}
}
