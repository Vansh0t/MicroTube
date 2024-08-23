using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MicroTube.Data.Models
{
    public class BasicFlowAuthenticationData: AuthenticationData
    {
		[Column(TypeName = "VARCHAR"), StringLength(100)]
		public required string PasswordHash { get; set; }
		[Column(TypeName = "VARCHAR"), StringLength(100)]
		public string? EmailConfirmationString { get; set; }
        public DateTime? EmailConfirmationStringExpiration { get; set; }
		[Column(TypeName = "VARCHAR"), StringLength(100)]
		public string? PasswordResetString { get; set; }
		public DateTime? PasswordResetStringExpiration { get; set; }
		[Column(TypeName = "VARCHAR"), StringLength(50)]
		public string? PendingEmail { get; set; }
    }
}
