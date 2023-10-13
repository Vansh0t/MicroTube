namespace MicroTube.Services.Cryptography
{
    public interface IPasswordEncryption
    {
        public string Encrypt(string password, byte[]? encryptionMeta = null);
        public bool Validate(string encryptedPassword, string rawPassword);
    }
}
