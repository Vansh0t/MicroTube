using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroTube.Data.Models
{
	public abstract class AuthenticationData
	{
		[Key]
		public Guid Id { get; set; }
		[Required]
		[ForeignKey(nameof(User))]
		public Guid UserId { get; set; }
		public AppUser? User { get; set; }
	}
}
