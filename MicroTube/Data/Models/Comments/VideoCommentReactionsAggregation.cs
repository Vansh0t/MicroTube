using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MicroTube.Data.Models.Comments
{
    public class VideoCommentReactionsAggregation: ILikeDislikeReactionsAggregation
	{
		[Key]
		public Guid Id { get; set; }
		[Required]
		[ForeignKey(nameof(Comment))]
		public Guid CommentId { get; set; }
		public VideoComment? Comment { get; set; }
		public required int Likes { get; set; }
		public required int Dislikes { get; set; }
	}
}
