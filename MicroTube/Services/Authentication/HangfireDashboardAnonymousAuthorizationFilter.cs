using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace MicroTube.Services.Authentication
{
	public class HangfireDashboardAnonymousAuthorizationFilter : IDashboardAuthorizationFilter
	{
		//TO DO: remove in production
		public bool Authorize([NotNull] DashboardContext context)
		{
			return true;
		}
	}
}
