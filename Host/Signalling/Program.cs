using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Signalling.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using SharedHost;
using Newtonsoft.Json;
using SharedHost.Models.Session;
using System.Net;
using System.IO;

namespace Signalling
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
            SeedSession(host);
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });


        static void SeedSession(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var sessionQueue = services.GetRequiredService<ISessionQueue>();
                var systemConfig = services.GetRequiredService<SystemConfig>();
                var config = services.GetRequiredService<IConfiguration>();

                var conductor = new RestClient(systemConfig.Conductor+"/System");
                var request = new RestRequest("SeedSession")
                    .AddJsonBody(systemConfig.AdminLogin);

                var response = conductor.Post(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var seededSession = JsonConvert.DeserializeObject<List<SessionPair>>(response.Content);
                    foreach(var i in seededSession)
                    {
                        sessionQueue.AddSessionPair(i);
                    }
                }
            }
        }
    }
}
