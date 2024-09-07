using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MicroTube.Tests.Mock
{
	public class StartupTestWebAppFactory: WebApplicationFactory<Program>
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			base.ConfigureWebHost(builder);
			builder.UseSetting("StartupTest", "true");
		}
	}
}
