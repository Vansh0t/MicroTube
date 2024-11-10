using MicroTube.Controllers.Reactions.Dto;
using MicroTube.Data.Models;

namespace MicroTube.Controllers.Videos.Dto
{
	public class VideoDto
	{
		public required string Id { get; set; }
		public required string Urls { get; set; }
		public required string Title { get; set; }
		public required DateTime UploadTime { get; set; }
		public string? Description { get; set; }
		public string? ThumbnailUrls { get; set; }
		public required int LengthSeconds { get; set; }
		public required LikeDislikeReactionsAggregationDto ReactionsAggregation { get; set; }
		public int Views { get; set; }
		public string? UploaderPublicUsername { get; set; }
		public string? UploaderId { get; set; }
		public required int CommentsCount { get; set; }
		public static VideoDto FromModel(Video video)
		{
			int likes = video.VideoReactions != null ? video.VideoReactions.Likes : 0;
			int dislikes = video.VideoReactions != null ? video.VideoReactions.Dislikes : 0;
			int difference = video.VideoReactions != null ? video.VideoReactions.Difference : 0;
			VideoDto dto = new VideoDto
			{
				Id = video.Id.ToString(),
				Title = video.Title,
				Urls = video.Urls,
				Description = video.Description,
				UploadTime = video.UploadTime,
				ThumbnailUrls = video.ThumbnailUrls,
				LengthSeconds = video.LengthSeconds,
				ReactionsAggregation = new LikeDislikeReactionsAggregationDto(video.Id.ToString(), likes, dislikes, difference),
				Views = video.VideoViews != null ? video.VideoViews.Views : 0,
				UploaderPublicUsername = video.Uploader != null ? video.Uploader.PublicUsername : "Unknown",
				UploaderId = video.UploaderId.ToString(),
				CommentsCount = video.CommentsCount
			};
			return dto;
			
		}

	}
}
