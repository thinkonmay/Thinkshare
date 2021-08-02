﻿using IdentityServer4.EntityFramework.Entities;
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

            builder.Entity<Session>().HasIndex(s => new { s.SessionClientID, s.SessionSlaveID }).IsUnique();
            builder.Entity<Session>().Property(s => s.StartTime).HasDefaultValueSql("getUtcDate()");
            builder.Entity<UserAccount>().Property(u => u.Created).HasDefaultValueSql("getUtcDate()");
        }

        public DbSet<Session> Sessions { get; set; }
        public DbSet<Slave> Devices { get; set; }
        public DbSet<CommandLog> CommandLogs { get; set; }
    }
}
