using MicroTube.Data.Models;

namespace MicroTube.Services.Authentication
{
	public interface IUserSessionService
	{
		Task<IServiceResult<NewSessionResult>> CreateNewSession(int userId);
		Task<IServiceResult<NewSessionResult>> RefreshSession(string refreshToken);
		Task InvalidateSession(AppUserSession session, string reason);
	}
}