using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MicroTube.Data.Models
{
	[Index(nameof(Ip), IsUnique = true)]
	public class VideoView
	{
		[Key]
		public Guid Id { get; set; }
		[Required]
		[ForeignKey(nameof(Video))]
		public Guid VideoId { get; set; }
		public Video? Video { get; set; }
		[Column(TypeName = "VARCHAR"), StringLength(50)]
		public required string Ip { get; set; }

	}
}
