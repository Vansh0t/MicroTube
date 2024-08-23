using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MicroTube.Data.Models
{
	[Index(nameof(Username), IsUnique = true)]
	[Index(nameof(Email), IsUnique = true)]
	public class AppUser
	{
		[Key]
		public Guid Id { get; set; }
		[StringLength(50)]
		[Column(TypeName = "VARCHAR")]
		public required string Username { get; set; }
		[StringLength(50)]
		[Column(TypeName = "VARCHAR")]
		public required string Email { get; set; }
		[StringLength(50)]
		[Column(TypeName = "NVARCHAR")]
		public required string PublicUsername { get; set; }
		public required bool IsEmailConfirmed { get; set; }
		[Required]
		[ForeignKey(nameof(Authentication))]
		public Guid AuthenticationId { get; set; }
		public AuthenticationData? Authentication { get; set; }
    }
}
