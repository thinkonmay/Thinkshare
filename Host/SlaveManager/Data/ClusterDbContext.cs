﻿using Microsoft.EntityFrameworkCore;
using SharedHost.Models.Session;
using SharedHost.Models.Cluster;
using DbSchema.SystemDb.Data;
using WorkerManager.SlaveDevices;
using SharedHost.Models.Shell;

namespace WorkerManager.Data
{
    /// <summary>
    /// Database context in ef framework, readmore at https://www.entityframeworktutorial.net/efcore/entity-framework-core-dbcontext.aspx
    /// </summary>
    public class ClusterDbContext : DbContext
    {
        public ClusterDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
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
            builder.Entity<Cluster>().Property(u => u.Register).HasDefaultValueSql("current_timestamp");
        }


        public DbSet<Cluster> Clusters { get; set; }
        public DbSet<ShellSession> CachedSession { get; set; }
        public DbSet<ClusterWorkerNode> Devices { get; set; }
        public DbSet<QoE> QoEs { get; set; }
    }
}
