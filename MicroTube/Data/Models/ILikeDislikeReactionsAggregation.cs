namespace MicroTube.Data.Models
{
    public interface ILikeDislikeReactionsAggregation
    {
        Guid Id { get; set; }
        int Likes { get; set; }
        int Dislikes { get; set; }
    }
}