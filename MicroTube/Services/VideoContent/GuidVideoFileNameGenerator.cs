namespace MicroTube.Services.VideoContent
{
	public class GuidVideoFileNameGenerator : IVideoFileNameGenerator
	{
		public string GenerateVideoName()
		{
			return Guid.NewGuid().ToString().Replace("-", "");
		}
	}
}
