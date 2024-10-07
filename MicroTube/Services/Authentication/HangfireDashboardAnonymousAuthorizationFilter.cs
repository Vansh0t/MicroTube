using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace MicroTube.Services.Authentication
{
	/// <summary>
	/// For development usage only.
	/// </summary>
	public class HangfireDashboardAnonymousAuthorizationFilter : IDashboardAuthorizationFilter
	{
		public bool Authorize([NotNull] DashboardContext context)
		{
			return true;
		}
	}
}
