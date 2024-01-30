using MicroTube.Data.Models;

namespace MicroTube.Services.Authentication
{
	public interface IUserSessionService
	{
		Task<IServiceResult<NewSessionResult>> CreateNewSession(string userId);
		Task<IServiceResult<NewSessionResult>> RefreshSession(string refreshToken);
		Task InvalidateSession(AppUserSession session, string reason);
	}
}