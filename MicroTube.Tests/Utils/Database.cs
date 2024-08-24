using EntityFramework.Exceptions.Sqlite;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MicroTube.Data.Access;
using MicroTube.Tests.Mock;

namespace MicroTube.Tests.Utils
{
	internal static class Database
	{
		public static MicroTubeDbContext CreateSqliteInMemory()
		{
			var conn = new SqliteConnection("DataSource=:memory:");
			conn.Open();
			var options = new DbContextOptionsBuilder<MicroTubeDbContext>()
			.UseSqlite(conn)
			.UseExceptionProcessor()
			.Options;
			var context = new MicroTubeDbContext(options);
			context.Database.EnsureCreated();
			return context;
		}
		public static MockDbContext CreateSqliteInMemoryMock()
		{
			var conn = new SqliteConnection("DataSource=:memory:");
			conn.Open();
			var options = new DbContextOptionsBuilder<MicroTubeDbContext>()
			.UseSqlite(conn)
			.UseExceptionProcessor()
			.Options;
			var context = new MockDbContext(options);
			context.Database.EnsureCreated();
			return context;
		}
	}
}
