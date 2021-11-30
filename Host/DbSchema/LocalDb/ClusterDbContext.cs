using Microsoft.EntityFrameworkCore;
using SharedHost.Models.Session;
using SharedHost.Models.Cluster;
using SharedHost.Models.Shell;
using SharedHost.Models.Local;

namespace DbSchema.SystemDb
{
    /// <summary>
    /// Database context in ef framework, readmore at https://www.entityframeworktutorial.net/efcore/entity-framework-core-dbcontext.aspx
    /// </summary>
    public class ClusterDbContext : DbContext
    {
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
            builder.Entity<LocalCluster>().Property(u => u.Register).HasDefaultValueSql("current_timestamp");
        }


        public DbSet<OwnerCredential> Owner { get; set; }
        public DbSet<LocalCluster> Clusters { get; set; }
        public DbSet<ShellSession> CachedSession { get; set; }
        public DbSet<ClusterWorkerNode> Devices { get; set; }
        public DbSet<ScriptModel> ScriptModels {get;set;}
    }
}
