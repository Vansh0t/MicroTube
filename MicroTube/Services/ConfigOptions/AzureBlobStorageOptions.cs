namespace MicroTube.Services.ConfigOptions
{
	public class AzureBlobStorageOptions
	{
		public static string KEY = "AzureBlobStorage";
		public string ConnectionString { get; set; }

		public AzureBlobStorageOptions(string connectionString)
		{
			ConnectionString = connectionString;
		}
	}
}
