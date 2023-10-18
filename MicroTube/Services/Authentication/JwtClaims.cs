using MicroTube.Data.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MicroTube.Services.Authentication
{
    public class JwtClaims:IJwtClaims
    {
        private const string PUBLIC_USERNAME_CLAIM_NAME = "public_name";
        private const string EMAIL_CONFIRMED_CLAIM_NAME = "email_confirmed";

        public Dictionary<string, string> GetClaims(AppUser user)
        {
            return new Dictionary<string, string>
            {
                { JwtRegisteredClaimNames.Sub, user.Id.ToString() },
                { EMAIL_CONFIRMED_CLAIM_NAME, user.IsEmailConfirmed.ToString() },
                { PUBLIC_USERNAME_CLAIM_NAME, user.PublicUsername.ToString() }
            };
        }
    }
}
