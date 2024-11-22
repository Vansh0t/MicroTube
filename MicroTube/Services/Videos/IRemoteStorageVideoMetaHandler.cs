
namespace MicroTube.Services.Videos
{
	public interface IRemoteStorageVideoMetaHandler
	{
		string? ReadDescription(IDictionary<string, string?> meta);
		string? ReadTitle(IDictionary<string, string?> meta);
		string? ReadUploaderId(IDictionary<string, string?> meta);
		public IDictionary<string, string?> WriteTitle(IDictionary<string, string?> meta, string title);
		public IDictionary<string, string?> WriteDescription(IDictionary<string, string?> meta, string? description);
		public IDictionary<string, string?> WriteUploaderId(IDictionary<string, string?> meta, string uploaderId);
	}
}