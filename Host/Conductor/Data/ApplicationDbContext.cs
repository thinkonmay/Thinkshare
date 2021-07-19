using Conductor.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedHost.Models;

namespace Conductor.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
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
            builder.Entity<SystemSession>().HasIndex(s => new { s.SessionClientID, s.SessionSlaveID }).IsUnique();
            builder.Entity<Telemetry>().HasOne(p => p.Device).WithMany(p => p.Telemetry);
        }

        public DbSet<SystemSession> Sessions { get; set; }
        public DbSet<Slave> Devices { get; set; }
        public DbSet<Telemetry> TelemetryLogs { get; set; }
    }
}
