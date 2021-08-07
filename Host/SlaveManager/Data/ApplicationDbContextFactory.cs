using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SlaveManager;

namespace SlaveManager.Data
{
    public class ApplicationDbContextFactory
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json")
                .Build();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseSqlServer($"Server={GeneralConstants.sql_server},{GeneralConstants.sql_port};Initial Catalog={GeneralConstants.sql_database};User ID={GeneralConstants.sql_user};Password={GeneralConstants.sql_password}");
            return new ApplicationDbContext(builder.Options);
        }
    }
}
