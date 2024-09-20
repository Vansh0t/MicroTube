using Hangfire.Dashboard;

namespace MicroTube.Services.Authentication
{
	public class AdminHangfireDashboardAuthorizationFilter: IDashboardAuthorizationFilter
	{
		private readonly IAdminJwtClaims _adminClaims = new AdminJwtClaims();
		public bool Authorize(DashboardContext context)
		{
			return _adminClaims.IsAdmin(context.GetHttpContext().User);
		}
	}
}
