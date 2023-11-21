using MicroTube.Data.Models;

namespace MicroTube.Data.Access
{
    public interface IUserSessionDataAccess
    {
        Task CreateSession(int userId, string token, DateTime issuedDateTime, DateTime expirationDateTime);
        Task<AppUserSession?> GetSessionByToken(string token);
        Task UpdateSession(AppUserSession session);
    }
}