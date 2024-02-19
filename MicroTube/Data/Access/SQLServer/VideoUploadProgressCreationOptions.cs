namespace MicroTube.Data.Access.SQLServer
{
	public class VideoUploadProgressCreationOptions
	{
		public string UploaderId { get; set; }
		public string RemoteCacheLocation { get; set; }
		public string RemoteCacheFileName { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }
		public VideoUploadProgressCreationOptions(string uploaderId, string remoteCacheLocation, string remoteCacheFileName, string title, string? description)
		{
			UploaderId = uploaderId;
			RemoteCacheLocation = remoteCacheLocation;
			RemoteCacheFileName = remoteCacheFileName;
			Title = title;
			Description = description;
		}
	}
}
