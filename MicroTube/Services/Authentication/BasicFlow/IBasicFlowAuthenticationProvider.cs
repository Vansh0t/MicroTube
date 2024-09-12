using MicroTube.Data.Models;

namespace MicroTube.Services.Authentication.BasicFlow
{
	public interface IBasicFlowAuthenticationProvider
	{
		Task<IServiceResult<AppUser>> CreateUser(string username, string email, string password);
		Task<IServiceResult<AppUser>> SignIn(string credential, string password);
	}
}