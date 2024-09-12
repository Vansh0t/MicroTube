using MicroTube.Data.Models;
using System.Security.Claims;

namespace MicroTube.Services.Authentication
{
    public interface IJwtClaims
    {
        public Dictionary<string, string> GetClaims(AppUser user);
        public string GetUserId(ClaimsPrincipal claimsBearer);
        public string GetUsername(ClaimsPrincipal claimsBearer);
        public bool GetIsEmailConfirmed(ClaimsPrincipal claimsBearer);
        public T GetRequiredClaim<T>(ClaimsPrincipal claimsBearer, string claimType);
        public bool TryGetClaim<T>(ClaimsPrincipal claimsBearer, string claimType, out T? claim);
    }
}
