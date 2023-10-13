using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace MicroTube.Services.Cryptography
{
    public class PBKDF2PasswordEncryption : IPasswordEncryption
    {
        private const int HASH_SALT_LENGTH = 128 / 8;
        private const int HASH_ITERATIONS = 10000;
        private const int HASH_KEY_LENGTH = 256 / 8;
        public string Encrypt(string password, byte[]? encryptionMeta = null)
        {
            byte[] salt = encryptionMeta != null ? encryptionMeta: RandomNumberGenerator.GetBytes(HASH_SALT_LENGTH);
            byte[] result = new byte[HASH_SALT_LENGTH + HASH_KEY_LENGTH];
            byte[] hashedPassword = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: HASH_ITERATIONS,
            numBytesRequested: HASH_KEY_LENGTH);
            salt.CopyTo(result, 0);
            hashedPassword.CopyTo(result, salt.Length);
            return Convert.ToBase64String(result);
        }

        public bool Validate(string encryptedPassword, string rawPassword)
        {
            byte[] hash = Convert.FromBase64String(encryptedPassword);
            byte[] salt = hash[..HASH_SALT_LENGTH];
            string rawToHash = Encrypt(rawPassword, salt);
            return rawToHash == encryptedPassword;
        }
    }
}
