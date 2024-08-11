namespace MicroTube.Data.Models
{
	public class VideoLike
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public required AppUser User { get; set; }
		public Guid VideoId { get; set; }
		public required Video Video { get; set; }
		public DateTime Time { get; set; }
	}
}
