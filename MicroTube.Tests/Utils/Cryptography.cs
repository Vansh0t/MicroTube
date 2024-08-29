using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroTube.Tests.Utils
{
    public static class Cryptography
    {
        public static Dictionary<string,string> ValidateAndGetClaimsFromJWTToken(string jwtToken, TokenValidationParameters validationParameters)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(jwtToken, validationParameters, out var validToken);
            var validJwtToken = (JwtSecurityToken)validToken;
            var claims = new Dictionary<string, string>();
            foreach(var claim in validJwtToken.Claims)
            {
                claims.Add(claim.Type, claim.Value);
            }
            return claims;
        }
    }
}
