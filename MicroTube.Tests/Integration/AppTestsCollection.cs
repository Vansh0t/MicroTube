using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroTube.Tests.Integration
{
    [CollectionDefinition(nameof(AppTestsCollection))]
    public class AppTestsCollection:ICollectionFixture<MicroTubeWebAppFactory<Program>>
    {
    }
}
