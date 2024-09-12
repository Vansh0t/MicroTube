namespace MicroTube.Services.VideoContent
{
	public class GuidVideoNameGenerator : IVideoNameGenerator
	{
		public string GenerateVideoName()
		{
			return Guid.NewGuid().ToString().Replace("-", "");
		}
	}
}
