using Microsoft.IdentityModel.Tokens;
using MicroTube.Data.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MicroTube.Services.Cryptography
{
    public class DefaultJwtTokenProvider : IJwtTokenProvider
    {
        

        private const string CONFIG_ISSUER_KEY = "JWT:Issuer";
        private const string CONFIG_AUDIENCE_KEY = "JWT:Audience";
        private const string CONFIG_KEY_KEY = "JWT:Key";
        private const string CONFIG_EXPIRATION_KEY = "JWT:ExpirationMinutes";

        private readonly IConfiguration _config;
        private readonly ILogger<DefaultJwtTokenProvider> _logger;

        public DefaultJwtTokenProvider(IConfiguration config, ILogger<DefaultJwtTokenProvider> logger)
        {
            _config = config;
            _logger = logger;
        }
        public IServiceResult<string> GetToken(Dictionary<string, string> claims)
        {
            string? issuer = _config[CONFIG_ISSUER_KEY];
            string? audience = _config[CONFIG_AUDIENCE_KEY];
            string? key = _config[CONFIG_KEY_KEY];
            int expirationMinutes;

            if(issuer == null || audience == null || key == null || !int.TryParse(_config[CONFIG_EXPIRATION_KEY], out expirationMinutes))
            {
                _logger.LogCritical("JWT misconfigured. Ensure IConfiguration has all necessary JWT values");
                return ServiceResult<string>.FailInternal();
            }
            if(claims == null || claims.Count <= 0)
            {
                _logger.LogCritical("Unable to generate a JWT: claims are empty");
                return ServiceResult<string>.FailInternal();
            }
            if(expirationMinutes <=0)
            {
                _logger.LogCritical("Unable to generate a JWT: expiration minutes <= 0");
                return ServiceResult<string>.FailInternal();
            }
            DateTime expirationTime = DateTime.UtcNow.AddMinutes(expirationMinutes);
            DateTime notBeforeTime = DateTime.UtcNow;
            var jwtSettings = new JwtSecurityToken(issuer, audience, claims.Select(_ => new Claim(_.Key, _.Value)), notBeforeTime, expirationTime,
                new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)), SecurityAlgorithms.HmacSha256));
            try
            {
                string jwt = new JwtSecurityTokenHandler().WriteToken(jwtSettings);
                return ServiceResult<string>.Success(jwt);
            }
            catch(Exception e)
            {
                _logger.LogCritical(e, "Unable to generate a JWT: unhandled exception.");
                return ServiceResult<string>.FailInternal();
            }
            
            
        }
    }
}
