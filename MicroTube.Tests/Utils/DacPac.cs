using Microsoft.SqlServer.Dac;

namespace MicroTube.Tests.Utils
{
    public static class DacPac
    {
        public static void DeployDacPac(string connString, string dacpacPath, string targetDbName)
        {
            var dbServices = new DacServices(connString);
            using FileStream dacFileStream = new FileStream(dacpacPath, FileMode.Open, FileAccess.Read);
            var dbPackage = DacPackage.Load(dacFileStream, DacSchemaModelStorageType.Memory, FileAccess.Read);

            var dbDeployOptions = new DacDeployOptions()
            {
                CreateNewDatabase = true,
                BlockOnPossibleDataLoss = false,
                BlockWhenDriftDetected = false
            };
            dbServices.Deploy(dbPackage, targetDbName, upgradeExisting: true, options: dbDeployOptions);
        }
    }
}
