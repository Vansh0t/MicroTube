using MicroTube.Controllers.Comments.Dto;
using MicroTube.Data.Models.Comments;

namespace MicroTube.Services.Comments.Reactions
{
	public class CommentDtoPair
	{
		public Guid Id => Comment.Id;
		public CommentDto CommentDto { get;set; }
		public IComment Comment { get; set; }
		public CommentDtoPair(CommentDto commentDto, IComment comment)
		{
			if(commentDto.Id != comment.Id.ToString())
			{
				throw new ArgumentException($"{nameof(CommentDtoPair)} must contain a {nameof(commentDto)} which corresponds to {nameof(comment)}. {commentDto.Id} != {comment.Id}");
			}
			CommentDto = commentDto;
			Comment = comment;
		}
	}
}
