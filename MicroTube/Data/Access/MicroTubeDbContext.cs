using Microsoft.EntityFrameworkCore;
using MicroTube.Data.Models;

namespace MicroTube.Data.Access
{
	public class MicroTubeDbContext: DbContext
	{
		public DbSet<AppUser> Users { get; set; }
		public DbSet<AppUserSession> UserSessions { get; set; }
		public DbSet<EmailPasswordAuthentication> EmailPasswordAuthentications { get; set; }
		public DbSet<UsedRefreshToken> UsedRefreshTokens { get; set; }
		public DbSet<Video> Videos { get; set; }
		public DbSet<VideoReactions> VideoReactions { get; set; }
		public DbSet<VideoViews> VideoViews { get; set; }
		public DbSet<VideoUploadProgress> VideoUploadProgresses { get; set; }
	}
}
