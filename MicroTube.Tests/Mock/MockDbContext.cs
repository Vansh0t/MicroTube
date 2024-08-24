using Microsoft.EntityFrameworkCore;
using MicroTube.Data.Access;
using MicroTube.Tests.Mock.Models;

namespace MicroTube.Tests.Mock
{
	public class MockDbContext: MicroTubeDbContext
	{
		public MockDbContext()
		{
		}

		public MockDbContext(DbContextOptions<MicroTubeDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<MockAuthenticationData>().ToTable("MockAuthenticationData");
		}
	}
}
