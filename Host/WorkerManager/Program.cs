using Microsoft.AspNetCore.Hosting;
using System;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using WorkerManager.Interfaces;
using WorkerManager.Services;
using System.Collections.Generic;
using DbSchema.SystemDb;
using SharedHost.Models.Shell;
using System.Net;
using SharedHost;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;
using DbSchema.LocalDb;

namespace WorkerManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            GetDefaultModel(host).Wait();
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

        static async Task GetDefaultModel(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var _db = scope.ServiceProvider.GetRequiredService<ClusterDbContext>();
                var _config = scope.ServiceProvider.GetRequiredService<IOptions<ClusterConfig>>().Value;
                Console.WriteLine(_config.ClusterHub);

                if(!_db.ScriptModels.Any())
                {
                    var _client = new RestClient();
                    var request = new RestRequest(new Uri(_config.ScriptModelUrl));
                    request.Method = Method.GET;

                    var result = await _client.ExecuteAsync(request);
                    if(result.StatusCode == HttpStatusCode.OK)
                    {
                        var allModel = JsonConvert.DeserializeObject<ICollection<ScriptModel>>(result.Content);
                        var defaultModel = allModel.Where(o => o.ID < (int)ScriptModelEnum.LAST_DEFAULT_MODEL).ToList();
                        _db.ScriptModels.AddRange(defaultModel);
                        await _db.SaveChangesAsync();
                    }
                }
            }
        }

    }
}
