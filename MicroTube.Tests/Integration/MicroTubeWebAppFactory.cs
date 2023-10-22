using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MicroTube.Services.Email;
using MicroTube.Tests.Mocks;
using MicroTube.Tests.Utils;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace MicroTube.Tests.Integration
{
    public class MicroTubeWebAppFactory<TEntry>: WebApplicationFactory<TEntry>
        where TEntry: class
    {
        private readonly IMessageSink diagnosticMessageSink;

        public MicroTubeWebAppFactory(IMessageSink diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            diagnosticMessageSink.OnMessage(new DiagnosticMessage("Creating test db"));
            TestDatabase.Create();
            builder.UseConfiguration(ConfigurationProvider.GetConfiguration());
            builder.ConfigureServices(services =>
            {
                //var config = services.SingleOrDefault(_=>_.ServiceType == typeof(IConfiguration));
                //if (config != null)
                //    services.Remove(config);
                //services.AddSingleton(ConfigurationProvider.GetConfiguration());

                var authEmailManager = services.SingleOrDefault(_ => _.ServiceType == typeof(IAuthenticationEmailManager));
                if (authEmailManager != null)
                    services.Remove(authEmailManager);
                services.AddSingleton<IAuthenticationEmailManager, MockAuthenticationEmailManager>();
            });

            builder.UseEnvironment("Development");
        }
        public override ValueTask DisposeAsync()
        {
            diagnosticMessageSink.OnMessage(new DiagnosticMessage("Dropping test db"));
            TestDatabase.Drop();
            return base.DisposeAsync();
        }
    }
}
