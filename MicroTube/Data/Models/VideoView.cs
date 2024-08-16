namespace MicroTube.Data.Models
{
	public class VideoView
	{
		public Guid Id { get; set; }
		public Guid VideoId { get; set; }
		public required Video Video { get; set; }

	}
}
