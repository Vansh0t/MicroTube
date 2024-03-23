namespace MicroTube.Services.MediaContentStorage
{
	public class OfflineRemoteStorageOptions
	{
		public string FullPath { get; set; }
		public OfflineRemoteStorageOptions(string fullPath)
		{
			FullPath = fullPath;
		}

	}
}
