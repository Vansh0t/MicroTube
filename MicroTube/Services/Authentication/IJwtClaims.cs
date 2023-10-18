using MicroTube.Data.Models;

namespace MicroTube.Services.Authentication
{
    public interface IJwtClaims
    {
        public Dictionary<string, string> GetClaims(AppUser user);
    }
}
