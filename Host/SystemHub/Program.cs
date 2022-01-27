using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SharedHost.Logging;
using System;

namespace SystemHub
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
                SharedHost.Logging.Log.Fatal("Error intializing","Authenticator",ex);
                return;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
