using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using DbSchema.SystemDb.Data;
using DbSchema.DbSeeding;
using System;
using SharedHost.Logging;

namespace Conductor
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
                var env = services.GetRequiredService<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();

                ScriptModelSeeder.SeedScriptModel(db, env);
            }
        }
    }
}
