using MicroTube.Data.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MicroTube.Services.Authentication
{
    public class JwtClaims : IJwtClaims
    {
        private const string PUBLIC_USERNAME_CLAIM_NAME = "public_name";
        private const string EMAIL_CONFIRMED_CLAIM_NAME = "email_confirmed";

        public bool TryGetClaim<T>(ClaimsPrincipal claimsBearer, string claimType, out T? claim)
        {
            claim = default;
            string? claimRaw = claimsBearer.FindFirstValue(claimType);
            if (claimRaw == null)
                return false;
            var conversionResult = Convert.ChangeType(claimRaw, typeof(T));
            if (conversionResult == null)
                return false;
            claim = (T)conversionResult;
            return claim != null;
        }

        public Dictionary<string, string> GetClaims(AppUser user)
        {
            return new Dictionary<string, string>
            {
                { JwtRegisteredClaimNames.Sub, user.Id.ToString() },
                { EMAIL_CONFIRMED_CLAIM_NAME, user.IsEmailConfirmed.ToString() },
                { PUBLIC_USERNAME_CLAIM_NAME, user.PublicUsername.ToString() }
            };
        }

        public T GetRequiredClaim<T>(ClaimsPrincipal claimsBearer, string claimType)
        {
            string? claimRaw = claimsBearer.FindFirstValue(claimType);
            if (claimRaw == null)
                throw new RequiredObjectNotFoundException($"Required claim {claimType} not found");
            var conversionResult = Convert.ChangeType(claimRaw, typeof(T));
            if (conversionResult == null)
                throw new RequiredObjectNotFoundException($"Required claim {claimType} has wrong type. Expecting type {typeof(T)}, claim {claimRaw}");
            return (T)conversionResult;
        }

        public int GetUserId(ClaimsPrincipal claimsBearer)
        {
            return GetRequiredClaim<int>(claimsBearer, JwtRegisteredClaimNames.Sub);
        }

        public string GetUsername(ClaimsPrincipal claimsBearer)
        {
            return GetRequiredClaim<string>(claimsBearer, PUBLIC_USERNAME_CLAIM_NAME);
        }

        public bool GetIsEmailConfirmed(ClaimsPrincipal claimsBearer)
        {
            return GetRequiredClaim<bool>(claimsBearer, EMAIL_CONFIRMED_CLAIM_NAME);
        }
    }
}
