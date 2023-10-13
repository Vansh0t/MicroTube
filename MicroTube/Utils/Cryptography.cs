using System.Security.Cryptography;

namespace MicroTube.Utils
{
    public class Cryptography
    {
        public static string GetSecureRandomString(int length)
        {
            var randomBytes = RandomNumberGenerator.GetBytes(length);
            var base64 = Convert.ToBase64String(randomBytes, new() { });
            return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}
