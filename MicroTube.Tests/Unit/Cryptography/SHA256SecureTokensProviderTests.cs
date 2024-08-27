using MicroTube.Services.Cryptography;

namespace MicroTube.Tests.Unit.Cryptography
{
	public class SHA256SecureTokensProviderTests
	{
		[Fact]
		public void GenerateSecureToken_Success()
		{
			ISecureTokensProvider provider = new SHA256SecureTokensProvider();
			string token = provider.GenerateSecureToken();
			Assert.NotEmpty(token);
			Assert.Equal(128, token.Length);
		}
		[Fact]
		public void HashSecureToken_Success()
		{
			string token = "4B7547CE964D07F749AE1F93C84880F7A1297EE128CA826A5F01085A3735146EBBF1155EE4EC396D90BDF99C6BEF601A66EC4572F9E9E3670FF8F5A53864FD1D";
			ISecureTokensProvider provider = new SHA256SecureTokensProvider();
			string hash = provider.HashSecureToken(token);
			Assert.NotEmpty(token);
			Assert.True(token.Length > 32);
		}
		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("not_base64")]
		[InlineData("4B7547CE964D07F749AE1F93C84880F7A1297EE128CA826A5F01085A3735146EBBF1155EE4EC396D90BDF99C6BEF601A66EC4572F9E9E3670FF8F5A53864FD1")]
		public void HashSecureToken_Fail(string? token)
		{
			ISecureTokensProvider provider = new SHA256SecureTokensProvider();
			Assert.ThrowsAny<FormatException>(()=>provider.HashSecureToken(token));
		}
	}
}
