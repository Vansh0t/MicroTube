namespace MicroTube.Data.Models.Reactions
{
    public interface IReaction
    {
        Guid Id { get; set; }
        Guid UserId { get; set; }
        AppUser? User { get; }
        Guid TargetId { get; set; }
        IReactable? Target { get; }
        DateTime Time { get; set; }
    }
}
