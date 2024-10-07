using System.Security.Claims;

namespace MicroTube.Services.Authentication
{
	public class AdminJwtClaims : JwtClaims, IAdminJwtClaims
	{
		private const string ADMIN_CLAIM = "is_admin";
		public bool IsAdmin(ClaimsPrincipal claimsBearer)
		{
			string? adminClaim = claimsBearer.FindFirstValue(ADMIN_CLAIM);
			if(!bool.TryParse(adminClaim, out var isAdmin))
			{
				return false;
			}
			return isAdmin;
		}
	}
}
