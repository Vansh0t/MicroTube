namespace MicroTube.Services.Cryptography
{
    public interface IJwtTokenProvider
    {
        public IServiceResult<string> GetToken(Dictionary<string, string> claims);
    }
}
