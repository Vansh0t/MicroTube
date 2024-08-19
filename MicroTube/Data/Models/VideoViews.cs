namespace MicroTube.Data.Models
{
	public class VideoViews
	{
		public required Guid VideoId { get; set; }
		public required int Views { get; set; }
	}
}
