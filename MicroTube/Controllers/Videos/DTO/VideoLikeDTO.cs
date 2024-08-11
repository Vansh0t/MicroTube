using MicroTube.Data.Models;

namespace MicroTube.Controllers.Videos.DTO
{
	public class VideoLikeDTO
	{
		public string Id { get; set; }
		public string UserId { get; set; }
		public string VideoId { get; set; }
		public DateTime Time { get; set; }
		public VideoLikeDTO(string id, string userId, string videoId, DateTime time)
		{
			Id = id;
			UserId = userId;
			VideoId = videoId;
			Time = time;
		}
		public static VideoLikeDTO FromModel(VideoLike model)
		{
			return new VideoLikeDTO
			(
				model.Id.ToString(),
				model.UserId.ToString(),
				model.VideoId.ToString(),
				model.Time
			);
		}
	}
}
