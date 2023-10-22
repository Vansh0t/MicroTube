namespace MicroTube.Services.Cryptography
{
    public interface IJwtPasswordResetTokenProvider
    {
        public IServiceResult<string> GetToken(string subject);
    }
}
