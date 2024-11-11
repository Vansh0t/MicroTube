namespace MicroTube.Data.Models.Comments
{
	public interface IComment
	{
		Guid Id { get; set; }
		string Content { get; set; }
		Guid UserId { get; set; }
		AppUser? User { get; set; }
		bool Edited { get; set; }
		bool Deleted { get; set; }
		Guid TargetId { get; set; }
		DateTime Time { get; set; }
	}
}