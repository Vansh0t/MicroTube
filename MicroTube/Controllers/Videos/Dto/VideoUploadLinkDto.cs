namespace MicroTube.Controllers.Videos.Dto
{
	public class VideoUploadLinkDto
	{
		public string Link { get; set; }
		public string GeneratedFileName { get; set; }
		public string GeneratedRemoteLocationName { get; set; }

		public VideoUploadLinkDto(string link, string generatedFileName, string generatedRemoteLocationName)
		{
			Link = link;
			GeneratedFileName = generatedFileName;
			GeneratedRemoteLocationName = generatedRemoteLocationName;
		}
	}
}
