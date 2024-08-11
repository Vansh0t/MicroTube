using MicroTube.Data.Models;

namespace MicroTube.Controllers.Videos.DTO
{
	public class VideoDTO
	{
		public required string Id { get; set; }
		public required string Urls { get; set; }
		public required string Title { get; set; }
		public required DateTime UploadTime { get; set; }
		public string? Description { get; set; }
		public string? ThumbnailUrls { get; set; }
		public string? SnapshotUrls { get; set; }
		public required int LengthSeconds { get; set; }
		public int Likes { get; set; }
		public static VideoDTO FromModel(Video video)
		{
			VideoDTO dto = new VideoDTO
			{
				Id = video.Id.ToString(),
				Title = video.Title,
				Urls = video.Urls,
				Description = video.Description,
				UploadTime = video.UploadTime,
				ThumbnailUrls = video.ThumbnailUrls,
				SnapshotUrls = video.SnapshotUrls,
				LengthSeconds = video.LengthSeconds,
				Likes = video.Likes
			};
			return dto;
			
		}

	}
}
