using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace MicroTube.Tests.Mock
{
	public class StartupTestWebAppFactory: WebApplicationFactory<Program>
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.ConfigureServices(_ => _.AddControllers().AddControllersAsServices());
			base.ConfigureWebHost(builder);
			builder.UseSetting("StartupTest", "true");
		}
	}
}
