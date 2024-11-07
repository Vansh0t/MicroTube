using MicroTube.Services.Reactions;

namespace MicroTube.Data.Models.Reactions
{
    public interface ILikeDislikeReaction : IReaction
    {
        LikeDislikeReactionType ReactionType { get; set; }
    }
}
