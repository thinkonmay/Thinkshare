using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RestSharp;
using SharedHost;
using SlaveManager.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace SlaveManager
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
            SeedDevices(host);
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        static void SeedDevices(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var slavePool = services.GetRequiredService<ISlavePool>();
                var systemConfig = services.GetRequiredService<SystemConfig>();
                var config = services.GetRequiredService<IConfiguration>();

                var conductor = new RestClient(systemConfig.Conductor + "/System");
                var request = new RestRequest("SeedDevices")
                    .AddJsonBody(systemConfig.AdminLogin);

                var response = conductor.Post(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var seededSession = JsonConvert.DeserializeObject<List<int>>(response.Content);
                    foreach (var i in seededSession)
                    {
                        slavePool.AddSlaveId(i);
                    }
                }
            }
        }
    }
}
