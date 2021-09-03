using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Extensions;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Conductor.Models;
using Conductor.Models.User;
using System.Threading.Tasks;
using SharedHost.Models.Session;
using SharedHost.Models.Command;
using SharedHost.Models.Error;
using SharedHost.Models.Device;

namespace Conductor.Data
{
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
            builder.Entity<ShellSession>().Property(u => u.StartTime).HasDefaultValueSql("current_timestamp");
            builder.Entity<GeneralError>().Property(u => u.ErrorTime).HasDefaultValueSql("current_timestamp");
            builder.Entity<Slave>().Property(u => u.Register).HasDefaultValueSql("current_timestamp");

            builder.Entity<RemoteSession>().HasKey(o => new { o.SessionSlaveID, o.SessionClientID });
            builder.Entity<GeneralError>().HasKey(s => new { s.Id });
        }


        public DbSet<Slave> Devices { get; set; }
        public DbSet<RemoteSession> RemoteSessions { get; set; }
        public DbSet<CommandLog> CommandLogs { get; set; }
        public DbSet<ShellSession> ShellSession { get; set; }
        public DbSet<GeneralError> Errors { get; set; }
    }
}
