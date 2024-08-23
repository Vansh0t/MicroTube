using MicroTube.Data.Models;

namespace MicroTube.Services.Authentication.BasicFlow
{
	public interface IBasicFlowPasswordHandler
	{
		Task<IServiceResult> ChangePassword(string userId, string newPassword);
		Task<IServiceResult> StartPasswordReset(string email);
		Task<IServiceResult<string>> UsePasswordResetString(string passwordResetString);
		BasicFlowAuthenticationData ApplyPasswordReset(BasicFlowAuthenticationData authData, out string resetString);
	}
}