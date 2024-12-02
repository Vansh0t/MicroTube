namespace MicroTube.Services.ConfigOptions
{
	public class AzureBlobStorageOptions
	{
		public static string KEY = "AzureBlobStorage";
		public string ConnectionString { get; set; }
		public IReadOnlyList<string> CorsAllowedOrigins { get; set; } = new List<string>();
		public IReadOnlyList<string> CorsAllowedMethods { get; set; } = new List<string>();

		public AzureBlobStorageOptions(string connectionString)
		{
			ConnectionString = connectionString;
		}
	}
}
