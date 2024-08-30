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
		public required int LengthSeconds { get; set; }
		public int Likes { get; set; }
		public int Dislikes { get; set; }
		public int Views { get; set; }
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
				LengthSeconds = video.LengthSeconds,
				Likes = video.VideoReactions != null? video.VideoReactions.Likes:0,
				Dislikes = video.VideoReactions != null ? video.VideoReactions.Dislikes : 0,
				Views = video.VideoViews != null ? video.VideoViews.Views : 0
			};
			return dto;
			
		}

	}
}
