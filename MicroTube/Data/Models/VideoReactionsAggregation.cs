using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroTube.Data.Models
{
	public class VideoReactionsAggregation
	{
		[Key]
		[Required]
		[ForeignKey(nameof(Video))]
		public Guid VideoId { get; set; }
		public Video? Video { get; set; }
		public required int Likes { get; set; }
		public required int Dislikes { get; set; }
	}
}
