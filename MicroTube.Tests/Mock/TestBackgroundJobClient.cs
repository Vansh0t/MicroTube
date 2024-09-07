using Hangfire;
using Hangfire.Annotations;
using Hangfire.Common;
using Hangfire.States;

namespace MicroTube.Tests.Mock
{
	public class TestBackgroundJobClient : IBackgroundJobClient
	{
		public Job? CreatedJob { get; private set; }
		public bool ChangeState([NotNull] string jobId, [NotNull] IState state, [CanBeNull] string expectedState)
		{
			throw new NotImplementedException();
		}

		public string Create([NotNull] Job job, [NotNull] IState state)
		{
			CreatedJob = job;
			return Guid.NewGuid().ToString();
		}
	}
}
