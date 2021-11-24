using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedHost.Models.User;
using SharedHost.Models.Session;
using SharedHost.Models.Shell;
using SharedHost.Models.Device;
using SharedHost.Models.Cluster;

namespace DbSchema.SystemDb.Data
{
    /// <summary>
    /// Database context in ef framework, readmore at https://www.entityframeworktutorial.net/efcore/entity-framework-core-dbcontext.aspx
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<UserAccount, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        { 
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserAccount>().Property(u => u.Created).HasDefaultValueSql("current_timestamp");
            builder.Entity<RemoteSession>().Property(u => u.StartTime).HasDefaultValueSql("current_timestamp");
            builder.Entity<ShellSession>().Property(u => u.Time).HasDefaultValueSql("current_timestamp");
            builder.Entity<WorkerNode>().Property(u => u.Register).HasDefaultValueSql("current_timestamp");
            builder.Entity<WorkerCluster>().Property(u => u.Register).HasDefaultValueSql("current_timestamp");
        }


        public DbSet<WorkerCluster> Clusters { get; set; }
        public DbSet<WorkerNode> Devices { get; set; }
        public DbSet<RemoteSession> RemoteSessions { get; set; }
        public DbSet<ShellSession> ShellSession { get; set; }
        public DbSet<ScriptModel> ScriptModels { get; set; }
        public DbSet<DeviceCap> DefaultSettings { get; set; }
    }
}
