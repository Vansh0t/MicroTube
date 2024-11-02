using System.Text.Json.Serialization;

namespace MicroTube.Services.Reactions
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LikeDislikeReactionType
    {
        None, Like, Dislike
    }
}
