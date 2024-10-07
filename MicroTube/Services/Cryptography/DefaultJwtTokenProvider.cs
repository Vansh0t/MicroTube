using Microsoft.IdentityModel.Tokens;
using MicroTube.Data.Models;
using MicroTube.Services.ConfigOptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MicroTube.Services.Cryptography
{
    public class DefaultJwtTokenProvider : IJwtTokenProvider
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DefaultJwtTokenProvider> _logger;

        public DefaultJwtTokenProvider(IConfiguration config, ILogger<DefaultJwtTokenProvider> logger)
        {
            _config = config;
            _logger = logger;
        }
        public IServiceResult<string> GetToken(Dictionary<string, string> claims)
        {
			var options = _config.GetRequiredByKey<JwtAccessTokensOptions>(JwtAccessTokensOptions.KEY);
            int expirationMinutes;
            if(claims == null || claims.Count <= 0)
            {
                _logger.LogCritical("Unable to generate a JWT: claims are empty");
                return ServiceResult<string>.FailInternal();
            }
            DateTime expirationTime = DateTime.UtcNow.AddMinutes(options.ExpirationMinutes);
            DateTime notBeforeTime = DateTime.UtcNow;
            var jwtSettings = new JwtSecurityToken(options.Issuer, options.Audience, claims.Select(_ => new Claim(_.Key, _.Value)), notBeforeTime, expirationTime,
                new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(options.Key)), SecurityAlgorithms.HmacSha256));
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
