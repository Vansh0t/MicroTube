namespace MicroTube.Services.Cryptography
{
    public interface IJwtPasswordResetTokenProvider
    {
        public string GetToken(string subject);
    }
}
