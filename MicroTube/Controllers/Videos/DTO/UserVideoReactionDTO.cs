using MicroTube.Data.Models;
using MicroTube.Services.Reactions;

namespace MicroTube.Controllers.Videos.DTO
{
	public class UserVideoReactionDTO
	{
		public required string UserId { get; set; }
		public required string VideoId { get; set; }
		public required DateTime Time { get; set; }
		public required LikeDislikeReactionType ReactionType { get; set; }
		public static UserVideoReactionDTO FromModel(UserVideoReaction model)
		{
			return new UserVideoReactionDTO
			{
				UserId = model.UserId.ToString(),
				VideoId = model.VideoId.ToString(),
				ReactionType = model.ReactionType,
				Time = model.Time
			};
		}
	}
}
