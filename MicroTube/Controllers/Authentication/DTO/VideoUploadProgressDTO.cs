using MicroTube.Data.Models;

namespace MicroTube.Controllers.Authentication.DTO
{
	public class VideoUploadProgressDTO
	{
		public string Id { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }
		public VideoUploadStatus Status { get; set; }
		public VideoUploadProgressDTO(string id, VideoUploadStatus status, string title, string? description)
		{
			Id = id;
			Status = status;
			Title = title;
			Description = description;
		}
	}
}
