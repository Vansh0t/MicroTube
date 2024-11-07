using MicroTube.Data.Models.Reactions;
using MicroTube.Services.Reactions;

namespace MicroTube.Controllers.Reactions.DTO
{
    public class LikeDislikeReactionDTO
    {
        public required string UserId { get; set; }
        public required string TargetId { get; set; }
        public required DateTime Time { get; set; }
        public required LikeDislikeReactionType ReactionType { get; set; }
        public static LikeDislikeReactionDTO FromModel(ILikeDislikeReaction model)
        {
            return new LikeDislikeReactionDTO
            {
                UserId = model.UserId.ToString(),
                TargetId = model.TargetId.ToString(),
                ReactionType = model.ReactionType,
                Time = model.Time
            };
        }
    }
}
