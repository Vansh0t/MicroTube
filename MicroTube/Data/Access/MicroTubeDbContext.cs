using Microsoft.EntityFrameworkCore;
using MicroTube.Data.Models;

namespace MicroTube.Data.Access
{
	public class MicroTubeDbContext: DbContext
	{
		public DbSet<AppUser> Users { get; set; }
		public DbSet<AppUserSession> UserSessions { get; set; }
		public DbSet<UserVideoReaction> UserVideoReactions { get; set; }
		public DbSet<UsedRefreshToken> UsedRefreshTokens { get; set; }
		public DbSet<Video> Videos { get; set; }
		public DbSet<VideoSearchIndexing> VideoSearchIndexing { get; set; }
		public DbSet<VideoReactionsAggregation> VideoAggregatedReactions { get; set; }
		public DbSet<VideoViewsAggregation> VideoAggregatedViews { get; set; }
		public DbSet<VideoView> VideoViews { get; set; }
		public DbSet<VideoUploadProgress> VideoUploadProgresses { get; set; }
		public DbSet<AuthenticationData> AuthenticationData { get; set; }
		public MicroTubeDbContext()
		{
		}
		public MicroTubeDbContext(DbContextOptions<MicroTubeDbContext> options):base(options)
		{
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<AuthenticationData>().UseTpcMappingStrategy();
			modelBuilder.Entity<BasicFlowAuthenticationData>().ToTable("BasicFlowAuthenticationData");
		}
	}
}
