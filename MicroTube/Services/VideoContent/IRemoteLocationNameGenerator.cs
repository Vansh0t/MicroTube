namespace MicroTube.Services.VideoContent
{
	public interface IRemoteLocationNameGenerator
	{
		string GetLocationName(string fileName);
	}
}