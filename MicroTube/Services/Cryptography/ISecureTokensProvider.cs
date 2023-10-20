namespace MicroTube.Services.Cryptography
{
    public interface ISecureTokensProvider
    {
        public string GenerateSecureToken();
        public string HashSecureToken(string secureToken);
        public bool Validate(string encryptedSecureToken, string secureTokenRaw);
    }
}
