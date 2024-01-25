using MicroTube.Data.Models;

namespace MicroTube.Data.Access
{
    public interface IUserSessionDataAccess
    {
        Task CreateSession(int userId, string token, DateTime issuedDateTime, DateTime expirationDateTime);
        Task<AppUserSession?> GetSessionByToken(string token);
        Task<AppUserSession?> GetSessionById(int sessionId);
        Task UpdateSession(AppUserSession session, IEnumerable<UsedRefreshToken>? newUsedRefreshTokens);
		Task<UsedRefreshToken?> GetUsedRefreshToken(string token);

	}
}