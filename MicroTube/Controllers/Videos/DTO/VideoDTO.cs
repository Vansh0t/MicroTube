using MicroTube.Controllers.Reactions.Dto;
using MicroTube.Data.Models.Videos;

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
			int likes = video.VideoReactionsAggregation != null ? video.VideoReactionsAggregation.Likes : 0;
			int dislikes = video.VideoReactionsAggregation != null ? video.VideoReactionsAggregation.Dislikes : 0;
			int difference = video.VideoReactionsAggregation != null ? video.VideoReactionsAggregation.Difference : 0;
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
				Views = video.VideoViewsAggregation != null ? video.VideoViewsAggregation.Views : 0,
				UploaderPublicUsername = video.Uploader != null ? video.Uploader.PublicUsername : "Unknown",
				UploaderId = video.UploaderId.ToString(),
				CommentsCount = video.CommentsCount
			};
			return dto;
			
		}

	}
}
