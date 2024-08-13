using MicroTube.Data.Models;

namespace MicroTube.Controllers.Videos.DTO
{
	public class VideoDislikeDTO
	{
		public string Id { get; set; }
		public string UserId { get; set; }
		public string VideoId { get; set; }
		public DateTime Time { get; set; }
		public VideoDislikeDTO(string id, string userId, string videoId, DateTime time)
		{
			Id = id;
			UserId = userId;
			VideoId = videoId;
			Time = time;
		}
		public static VideoDislikeDTO FromModel(VideoDislike model)
		{
			return new VideoDislikeDTO
			(
				model.Id.ToString(),
				model.UserId.ToString(),
				model.VideoId.ToString(),
				model.Time
			);
		}
	}
}
