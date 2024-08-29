using Microsoft.IdentityModel.Tokens;
using MicroTube.Constants;
using MicroTube.Services.ConfigOptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MicroTube.Services.Cryptography
{
    public class DefaultJwtPasswordResetTokenProvider : IJwtPasswordResetTokenProvider
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DefaultJwtPasswordResetTokenProvider> _logger;

        public DefaultJwtPasswordResetTokenProvider(IConfiguration config, ILogger<DefaultJwtPasswordResetTokenProvider> logger)
        {
            _config = config;
            _logger = logger;
        }
        public string GetToken(string subject)
        {
			var options = _config.GetRequiredByKey<JwtAccessTokensOptions>(JwtAccessTokensOptions.KEY);
            DateTime expirationTime = DateTime.UtcNow.AddMinutes(options.ExpirationMinutes);
            DateTime notBeforeTime = DateTime.UtcNow;
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, subject),
                new Claim(AuthorizationConstants.PASSWORD_RESET_CLAIM, "true")
            };
            var jwtSettings = new JwtSecurityToken(options.Issuer, options.Audience, claims, notBeforeTime, expirationTime,
                new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(options.Key)), SecurityAlgorithms.HmacSha256));
            string jwt = new JwtSecurityTokenHandler().WriteToken(jwtSettings);
            return jwt;
        }
    }
}
