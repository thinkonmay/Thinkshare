using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Conductor.Models.User;
using Conductor.Services;
using System.Threading.Tasks;
using SharedHost;
using System.IO;

namespace Conductor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    // find the shared folder in the parent folder
                    var sharedFolder = Path.Combine(env.ContentRootPath, "..", "SharedHost");

                    //load the SharedSettings first, so that appsettings.json overrwrites it
                    config
                        .AddJsonFile(Path.Combine(sharedFolder, "SharedSettings.json"), optional: true) // When running using dotnet run
                        .AddJsonFile("SharedSettings.json", optional: true) // When app is published
                        .AddJsonFile("appsettings.json", optional: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

                    config.AddEnvironmentVariables();
                })
                .Build();
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
                var systemconfig = services.GetRequiredService<SystemConfig>();
                var config = services.GetRequiredService<IConfiguration>();

                DataSeeder.SeedRoles(roleManager);
                DataSeeder.SeedAdminUsers(userManager,systemconfig);
                DataSeeder.SeedUserRole(userManager);
            }
        }
    }
}
