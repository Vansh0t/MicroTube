using MicroTube.Data.Models;

namespace MicroTube.Controllers.Authentication.DTO
{
	public class VideoUploadProgressDTO
	{
		public string Id { get; set; }
		public VideoUploadStatus Status { get; set; }
		public VideoUploadProgressDTO(string id, VideoUploadStatus status)
		{
			Id = id;
			Status = status;
		}
	}
}
