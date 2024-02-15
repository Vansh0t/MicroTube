using MicroTube.Data.Models;

namespace MicroTube.Data.Access
{
    public interface IUserSessionDataAccess
    {
        Task CreateSession(string userId, string token, DateTime issuedDateTime, DateTime expirationDateTime);
        Task<AppUserSession?> GetSessionByToken(string token);
        Task<AppUserSession?> GetSessionById(string sessionId);
        Task UpdateSession(AppUserSession session, IEnumerable<UsedRefreshToken>? newUsedRefreshTokens);
		Task<UsedRefreshToken?> GetUsedRefreshToken(string token);

	}
}