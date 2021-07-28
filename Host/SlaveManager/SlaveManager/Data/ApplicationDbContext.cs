using SlaveManager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedHost.Models;
using SlaveManager.Models.User;
using Microsoft.AspNetCore.Identity;
using IdentityServer4.EntityFramework.Interfaces;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using IdentityServer4.EntityFramework.Options;
using Microsoft.Extensions.Options;
using IdentityServer4.EntityFramework.Extensions;

namespace SlaveManager.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserAccount, IdentityRole<int>, int>, IPersistedGrantDbContext
    {
        private readonly IOptions<OperationalStoreOptions> _operationalStoreOptions;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IOptions<OperationalStoreOptions> operationalStoreOptions)
            : base(options)
        {
            _operationalStoreOptions = operationalStoreOptions;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ConfigurePersistedGrantContext(_operationalStoreOptions.Value);

            builder.Entity<Session>().HasIndex(s => new { s.SessionClientID, s.SessionSlaveID }).IsUnique();
            builder.Entity<Telemetry>().HasOne(p => p.Device).WithMany(p => p.Telemetry);
        }

        public async Task<int> SaveChangesAsync() => await base.SaveChangesAsync();


        public DbSet<Session> Sessions { get; set; }
        public DbSet<Slave> Devices { get; set; }
        public DbSet<Telemetry> TelemetryLogs { get; set; }
        public DbSet<CommandLog> CommandLogs { get; set; }

        public DbSet<PersistedGrant> PersistedGrants { get; set; }
        public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; }
    }
}
