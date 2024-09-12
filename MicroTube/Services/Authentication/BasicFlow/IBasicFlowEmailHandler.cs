using MicroTube.Data.Models;

namespace MicroTube.Services.Authentication.BasicFlow
{
	public interface IBasicFlowEmailHandler
	{
		BasicFlowAuthenticationData ApplyEmailConfirmation(BasicFlowAuthenticationData authData, out string confirmationString);
		Task<IServiceResult<AppUser>> ConfirmEmail(string stringRaw);
		Task<IServiceResult<AppUser>> ConfirmEmailChange(string stringRaw);
		Task<IServiceResult> ResendEmailConfirmation(string userId);
		Task<IServiceResult> StartEmailChange(string userId, string newEmail, string password);
	}
}