using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SlaveManager.Models.User;
using SlaveManager.Services;
using System.Threading.Tasks;

namespace SlaveManager
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
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        static void SeedDatabase(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var userManager = services.GetRequiredService<UserManager<UserAccount>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
                var config = services.GetRequiredService<IConfiguration>();

                DataSeeder.SeedRoles(roleManager);
                DataSeeder.SeedAdminUsers(userManager);
                DataSeeder.SeedUserRole(userManager);
            }
        }
    }
}
