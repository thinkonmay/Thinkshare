using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedHost.Models.User;
using SharedHost.Models.Session;
using SharedHost.Models.Shell;
using SharedHost.Models.Device;

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
            builder.Entity<Slave>().Property(u => u.Register).HasDefaultValueSql("current_timestamp");
            builder.Entity<RemoteSession>().HasKey(o => new { o.SessionSlaveID, o.SessionClientID });
        }

        
        public DbSet<Slave> Devices { get; set; }
        public DbSet<RemoteSession> RemoteSessions { get; set; }
        public DbSet<ShellSession> ShellSession { get; set; }
        public DbSet<ScriptModel> ScriptModels { get; set; }
        public DbSet<DeviceCap> DefaultSettings { get; set; }
    }
}
