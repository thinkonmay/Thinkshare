using DbSchema.LocalDb.Models;
using Microsoft.EntityFrameworkCore;
using SharedHost.Models.Cluster;
using SharedHost.Models.Shell;

namespace DbSchema.LocalDb
{
    /// <summary>
    /// Database context in ef framework, readmore at https://www.entityframeworktutorial.net/efcore/entity-framework-core-dbcontext.aspx
    /// </summary>
    public class ClusterDbContext : DbContext
    {
        private DbSet<ShellSession> cachedSession;

        public ClusterDbContext(DbContextOptions<ClusterDbContext> options) : base(options)
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

            builder.Entity<ClusterWorkerNode>().Property(u => u.Register).HasDefaultValueSql("current_timestamp");
            builder.Entity<ShellSession>().Property(u => u.Time).HasDefaultValueSql("current_timestamp");
            builder.Entity<LocalCluster>().Property(u => u.Register).HasDefaultValueSql("current_timestamp");
            builder.Entity<Log>().Property(u => u.LogTime).HasDefaultValueSql("current_timestamp");
        }


        public DbSet<OwnerCredential> Owner { get; set; }
        public DbSet<LocalCluster> Clusters { get; set; }
        public DbSet<ShellSession> CachedSession { get => cachedSession; set => cachedSession = value; }
        public DbSet<ClusterWorkerNode> Devices { get; set; }
        public DbSet<ScriptModel> ScriptModels { get; set; }
        public DbSet<Log> Logs { get; set; }
    }
}
