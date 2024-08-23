using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MicroTube.Data.Models
{
	[Index(nameof(UserId), nameof(VideoId), IsUnique =true)]
	public class UserVideoReaction
	{
		public Guid Id { get; set; }
		[Required]
		[ForeignKey(nameof(User))]
		public Guid UserId { get; set; }
		public AppUser? User { get; set; }
		[Required]
		[ForeignKey(nameof(Video))]
		public Guid VideoId { get; set; }
		public Video? Video { get; set; }
		public required ReactionType ReactionType { get; set; }
		public required DateTime Time { get; set; }
	}
	[JsonConverter(typeof(StringEnumConverter))]
	public enum ReactionType
	{
		None, Like, Dislike
	}
}
