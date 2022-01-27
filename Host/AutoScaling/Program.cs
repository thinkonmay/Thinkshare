using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SharedHost.Logging;
using System;

namespace AutoScaling
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = null;
            try
            {
                host = CreateHostBuilder(args).Build();
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
    }
}
