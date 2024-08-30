using System.Security.Cryptography;

namespace MicroTube.Services.Cryptography
{
    public class SHA256SecureTokensProvider : ISecureTokensProvider
    {
        private const int SECURE_TOKEN_SIZE_BYTES = 64;

        public string GenerateSecureToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(SECURE_TOKEN_SIZE_BYTES);
            //var base64 = Convert.ToBase64String(randomBytes, new() { });
            return Convert.ToHexString(randomBytes);
        }

        public string HashSecureToken(string secureToken)
        {
			if(string.IsNullOrWhiteSpace(secureToken))
				throw new FormatException($"Secure token must not be null or empty string.");
			byte[] secureTokenBytes = Convert.FromHexString(secureToken);
            if (secureTokenBytes.Length < SECURE_TOKEN_SIZE_BYTES)
                throw new FormatException($"Secure token length must be >= {SECURE_TOKEN_SIZE_BYTES}. Got: {secureTokenBytes.Length}");

            SHA256 hashingProvider = SHA256.Create();
            byte[] resultBytes = hashingProvider.ComputeHash(secureTokenBytes);
            return Convert.ToBase64String(resultBytes);
        }

        public bool Validate(string encryptedSecureToken, string secureTokenRaw)
        {
            string hash = HashSecureToken(secureTokenRaw);
            return encryptedSecureToken == hash;
        }
    }
}
