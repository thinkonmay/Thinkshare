using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedHost.Models.User;
using Conductor.Services;
using Conductor.Data;
using SharedHost.Models.Auth;
using SharedHost;
using Serilog;
using Serilog.Formatting.Elasticsearch;

namespace Conductor
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
                .UseSerilog((ctx, config) =>{
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

                var db = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<UserAccount>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
                var systemconfig = services.GetRequiredService<SystemConfig>();
                var config = services.GetRequiredService<IConfiguration>();

                ScriptModelSeeder.SeedScriptModel(db);
                AccountSeeder.SeedRoles(roleManager);
                AccountSeeder.SeedAdminUsers(userManager,systemconfig);
                AccountSeeder.SeedUserRole(userManager);
            }
        }
    }
}
