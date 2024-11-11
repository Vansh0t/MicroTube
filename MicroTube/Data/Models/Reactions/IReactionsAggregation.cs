namespace MicroTube.Data.Models.Reactions
{
	public interface IReactionsAggregation
	{
		Guid TargetId { get; set; }
		IReactable? Target { get; }
	}
}
