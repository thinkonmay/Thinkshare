<<<<<<< Updated upstream:Host/Conductor/Data/ApplicationDbContext.cs
﻿using Host.Models;
=======
﻿using SlaveManager.Models;
>>>>>>> Stashed changes:Host/SlaveManager/SlaveManager/Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlaveManager.Data
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
<<<<<<< Updated upstream:Host/Conductor/Data/ApplicationDbContext.cs
=======

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<SystemSession>().HasIndex(s => new { s.SessionClientID, s.SessionSlaveID }).IsUnique();
            builder.Entity<Telemetry>().HasOne(p => p.Device).WithMany(p => p.Telemetry);
        }

        public DbSet<SystemSession> Sessions { get; set; }
        public DbSet<Slave> Devices { get; set; }
        public DbSet<Telemetry> TelemetryLogs { get; set; }
        public DbSet<CommandLog> CommandLogs { get; set; }
>>>>>>> Stashed changes:Host/SlaveManager/SlaveManager/Data/ApplicationDbContext.cs
    }
}
