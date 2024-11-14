using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MicroTube.Data.Access;

namespace MicroTube.Data.Design
{
	public class DesignDbContextFactory: IDesignTimeDbContextFactory<MicroTubeDbContext>
	{
		public MicroTubeDbContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<MicroTubeDbContext>();
			optionsBuilder.UseSqlServer();
			return new MicroTubeDbContext(optionsBuilder.Options);
		}
	}
}
