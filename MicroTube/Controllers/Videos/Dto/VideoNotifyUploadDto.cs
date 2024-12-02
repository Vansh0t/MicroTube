namespace MicroTube.Controllers.Videos.Dto
{
    public class VideoNotifyUploadDto
	{
		public string GeneratedFileName { get; set; }
        public string GeneratedLocationName { get; set; }
		public VideoNotifyUploadDto(string generatedFileName, string generatedLocationName)
		{
			GeneratedFileName = generatedFileName;
			GeneratedLocationName = generatedLocationName;
		}
    }
}
