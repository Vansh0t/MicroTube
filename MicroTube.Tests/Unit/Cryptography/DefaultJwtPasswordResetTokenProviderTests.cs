using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.Cryptography;
using NSubstitute;
using System.Text;

namespace MicroTube.Tests.Unit.Cryptography
{
	public class DefaultJwtPasswordResetTokenProviderTests
	{
		[Fact]
		public void GetToken_Success()
		{
			string subject = Guid.NewGuid().ToString();
			string issuer = "issuer";
			string audience = "audience";
			int expirationMinutes = 5;
			string key = "testkey.11111.11111111.22222222222.261254r2.fw261rfsh5k65oke.671";
			var config = new ConfigurationBuilder().
				AddConfigObject(JwtAccessTokensOptions.KEY, new JwtAccessTokensOptions(
					issuer,
					audience,
					25,
					key,
					expirationMinutes))
				.Build();
			var tokenProvider = new DefaultJwtPasswordResetTokenProvider(config, Substitute.For<ILogger<DefaultJwtPasswordResetTokenProvider>>());
			var token = tokenProvider.GetToken(subject);
			Assert.False(string.IsNullOrWhiteSpace(token));
			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateLifetime = true,
				ValidateAudience = true,
				ValidateIssuer = true,
				ValidAudience = audience,
				ValidIssuer = issuer,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key))
			};
			var claims = Utils.Cryptography.ValidateAndGetClaimsFromJWTToken(token, tokenValidationParameters);
			Assert.Contains("password_reset", claims);
			Assert.Equal("true", claims["password_reset"]);
			Assert.Contains("sub", claims);
			Assert.Equal(subject, claims["sub"]);
			Assert.Contains("sub", claims);
			Assert.Contains("exp", claims);
			Assert.Contains("nbf", claims);
			int nbf = int.Parse(claims["nbf"]);
			int exp = int.Parse(claims["exp"]);
			Assert.Equal(expirationMinutes * 60, exp - nbf);
		}
	}
}
