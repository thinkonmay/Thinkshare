using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using DbSchema.SystemDb.Data;
using Microsoft.AspNetCore.Identity;
using SharedHost;
using Microsoft.Extensions.DependencyInjection;
using SharedHost.Models.User;
using DbSchema.DbSeeding;
using System;
using SharedHost.Logging;

namespace Authenticator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = null;
            try
            {
                host = CreateHostBuilder(args).Build();
                SeedDatabase(host);
            }
            catch(Exception ex)
            {
                Log.Fatal("Error intializing","Authenticator",ex);
                return;
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
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
                var systemconfig = services.GetRequiredService<IOptions<SystemConfig>>();

                
                AccountSeeder.SeedRoles(roleManager);
                AccountSeeder.SeedUserRole(userManager);
                AccountSeeder.SeedAdminUsers(userManager,db,systemconfig.Value);
            }
        }
    }
}
