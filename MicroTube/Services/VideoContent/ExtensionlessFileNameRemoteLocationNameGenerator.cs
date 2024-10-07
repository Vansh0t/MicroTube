using Ardalis.GuardClauses;

namespace MicroTube.Services.VideoContent
{
	public class ExtensionlessFileNameRemoteLocationNameGenerator : IRemoteLocationNameGenerator
	{
		public string GetLocationName(string fileName)
		{
			Guard.Against.NullOrWhiteSpace(fileName);
			return Path.GetFileNameWithoutExtension(fileName);
		}
	}
}
