using Microsoft.EntityFrameworkCore;
using Signalling.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Signalling.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Session>().Property(o => o.Created).HasDefaultValueSql("getdate()");
        }

        public DbSet<Session> Sessions { get; set; }
    }
}
