namespace MicroTube.Services.Videos
{
	public class DefaultRemoteStorageVideoMetaHandler : IRemoteStorageVideoMetaHandler
	{
		private const string TITLE_KEY = "Title";
		private const string DESCRIPTION_KEY = "Description";
		private const string UPLOADER_ID_KEY = "UploaderId";
		public string? ReadTitle(IDictionary<string, string?> meta)
		{
			meta.TryGetValue(TITLE_KEY, out var title);
			return title;
		}
		public string? ReadDescription(IDictionary<string, string?> meta)
		{
			meta.TryGetValue(DESCRIPTION_KEY, out var description);
			return description;
		}
		public string? ReadUploaderId(IDictionary<string, string?> meta)
		{
			meta.TryGetValue(UPLOADER_ID_KEY, out var uploaderId);
			return uploaderId;
		}
		public IDictionary<string, string?> WriteTitle(IDictionary<string, string?> meta, string title)
		{
			meta.Add(TITLE_KEY, title);
			return meta;
		}
		public IDictionary<string, string?>WriteDescription(IDictionary<string, string?> meta, string? description)
		{
			meta.Add(DESCRIPTION_KEY, description);
			return meta;
		}
		public IDictionary<string, string?>WriteUploaderId(IDictionary<string, string?> meta, string uploaderId)
		{
			meta.Add(UPLOADER_ID_KEY, uploaderId);
			return meta;
		}
	}
}
