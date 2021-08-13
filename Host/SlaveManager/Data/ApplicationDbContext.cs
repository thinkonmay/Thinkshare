using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Extensions;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SlaveManager.Models;
using SlaveManager.Models.User;
using System.Threading.Tasks;

namespace SlaveManager.Data
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
            builder.Entity<Slave>().Property(u => u.Register).HasDefaultValueSql("current_timestamp");

            builder.Entity<Session>().HasKey(o => new { o.SessionSlaveID, o.SessionClientID });
            builder.Entity<GeneralError>().HasKey(s => new { s.Id });
            builder.Entity<SessionCoreExit>().HasKey(s => new { s.Id });
        }


        public DbSet<Session> Sessions { get; set; }
        public DbSet<Slave> Devices { get; set; }
        public DbSet<CommandLog> CommandLogs { get; set; }
        public DbSet<GeneralError> GeneralErrors { get; set; }
        public DbSet<SessionCoreExit> SessionCoreExits { get; set; }
    }
}
