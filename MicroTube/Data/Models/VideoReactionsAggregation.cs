using MicroTube.Data.Models.Reactions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroTube.Data.Models
{
	public class VideoReactionsAggregation: ILikeDislikeReactionsAggregation
	{
		[Key]
		public Guid Id { get; set; }
		[Required]
		[ForeignKey(nameof(Video))]
		public Guid VideoId { get; set; }
		public Video? Video { get; set; }
		public required int Likes { get; set; }
		public required int Dislikes { get; set; }
		public int Difference { get; set; }

		[NotMapped]
		public Guid TargetId { get => VideoId; set => VideoId = value; }
		[NotMapped]
		public IReactable? Target => Video;
	}
}
