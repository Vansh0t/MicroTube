using System.Security.Claims;

namespace MicroTube.Services.Authentication
{
	public interface IAdminJwtClaims: IJwtClaims
	{
		bool IsAdmin(ClaimsPrincipal claimsBearer);
	}
}
