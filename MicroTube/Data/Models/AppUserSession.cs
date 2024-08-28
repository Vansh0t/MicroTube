using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroTube.Data.Models
{
	[Index(nameof(Token), IsUnique = true)]
	public class AppUserSession
	{
		[Key]
		public Guid Id { get; set; }
		[Required]
		[ForeignKey(nameof(User))]
		public Guid UserId { get; set; }
		public AppUser? User { get; set; }
		[Column(TypeName = "VARCHAR")]
		[StringLength(50)]
		public required string Token { get; set; }
		public required DateTime IssuedAt { get; set; }
		public required DateTime Expiration { get; set; }
		public required bool IsInvalidated { get; set; }
		public IList<UsedRefreshToken> UsedTokens { get; set; } = new List<UsedRefreshToken>();
	}
}
