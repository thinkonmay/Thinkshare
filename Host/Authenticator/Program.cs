using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Authenticator.Services;
using DbSchema;
using DbSchema.SystemDb.Data;
using Microsoft.AspNetCore.Identity;
using SharedHost;
using AutoMapper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedHost.Models.User;
using DbSchema.DbSeeding;

namespace Authenticator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            SeedDatabase(host);
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((ctx, config) => {
                    config
                        .MinimumLevel.Information()
                        .Enrich.FromLogContext();

                    config.WriteTo.Console();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        static void SeedDatabase(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var db = services.GetRequiredService<GlobalDbContext>();
                var userManager = services.GetRequiredService<UserManager<UserAccount>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
                var systemconfig = services.GetRequiredService<SystemConfig>();

                
                AccountSeeder.SeedRoles(roleManager);
                AccountSeeder.SeedAdminUsers(userManager,systemconfig);
                AccountSeeder.SeedUserRole(userManager);
            }
        }
    }
}
