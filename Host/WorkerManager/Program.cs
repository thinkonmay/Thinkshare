using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using SharedHost.Models.Device;
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
using DbSchema.CachedState;
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
            // if(Environment.GetEnvironmentVariable("AUTO_START") == "true")
            // {
                AutoStart(host).Wait();
            // }
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
                var _config = scope.ServiceProvider.GetRequiredService<IOptions<ClusterConfig>>().Value;
                var _cache  = scope.ServiceProvider.GetRequiredService<ILocalStateStore>();
                Console.WriteLine(_config.ClusterHub);
                var _client = new RestClient();
                var request = new RestRequest(new Uri(_config.ScriptModelUrl));
                request.Method = Method.GET;

                var result = await _client.ExecuteAsync(request);
                if(result.StatusCode == HttpStatusCode.OK)
                {
                    var allModel = JsonConvert.DeserializeObject<ICollection<ScriptModel>>(result.Content);
                    var defaultModel = allModel.Where(o => o.ID < (int)ScriptModelEnum.LAST_DEFAULT_MODEL).ToList();
                    await _cache.CacheScriptModel(defaultModel);
                }
            }
        }

        static async Task AutoStart(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var conductor = scope.ServiceProvider.GetRequiredService<IConductorSocket>();
                var pool      = scope.ServiceProvider.GetRequiredService<IWorkerNodePool>();
                var _cache    = scope.ServiceProvider.GetRequiredService<ILocalStateStore>();
                var _cluster  = await _cache.GetClusterInfor();

                var nodes = (await _cache.GetClusterInfor()).WorkerNodes;
                var initState = new Dictionary<int,string>();
                nodes.ForEach(x => initState.Add(x.ID,WorkerState.Disconnected));
                await _cache.SetClusterState(initState);

                if(_cluster.ClusterToken != null &&
                   _cluster.OwnerToken != null)
                {
                    if(await conductor.Start())
                    {
                        pool.Start();
                    }
                }
            }
        }
    }
}
