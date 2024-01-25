using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace MicroTube.Tests
{
    public static class ConfigurationProvider
    {
        public static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .Build();
        }
    }
}
