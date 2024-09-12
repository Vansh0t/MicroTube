using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MicroTube.Data.Models
{
	public class UsedRefreshToken
	{
		public Guid Id { get; set; }
		[Required]
		[ForeignKey(nameof(Session))]
		public Guid SessionId { get; set; }
		public AppUserSession? Session { get; set; }
		[Column(TypeName = "VARCHAR"), StringLength(100)]
		public required string Token { get; set; }
	}
}
