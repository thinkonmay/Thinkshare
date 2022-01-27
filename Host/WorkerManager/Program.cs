using Microsoft.AspNetCore.Hosting;
using SharedHost.Models.Device;
using System;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using WorkerManager.Interfaces;
using System.Collections.Generic;
using SharedHost.Models.Shell;
using System.Net;
using SharedHost;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;
using WorkerManager.Services;


namespace WorkerManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = null;
            try
            {
                host = CreateHostBuilder(args).Build();
                InitLocalStateStore(host).Wait();
                GetDefaultModel(host).Wait();
                AutoStart(host).Wait();
            }
            catch(Exception ex)
            {
                Log.Fatal("Error intializing","Cluster",ex);
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

        static async Task GetDefaultModel(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var _config = scope.ServiceProvider.GetRequiredService<IOptions<ClusterConfig>>().Value;
                var _cache  = scope.ServiceProvider.GetRequiredService<ILocalStateStore>();
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

        static async Task InitLocalStateStore(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var _config = scope.ServiceProvider.GetRequiredService<IOptions<ClusterConfig>>().Value;
                var _cache  = scope.ServiceProvider.GetRequiredService<ILocalStateStore>();
                var cluster = await _cache.GetClusterInfor();

                if(cluster == null || cluster.WorkerNodes == null)
                {
                    var initcluster  = new Models.ClusterKey();
                    initcluster.WorkerNodes = new List<Models.ClusterWorkerNode>();
                    await _cache.SetClusterInfor(initcluster);
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
                var _infor    = scope.ServiceProvider.GetRequiredService<IClusterInfor>();
                var _Port    =  scope.ServiceProvider.GetRequiredService<IPortProxy>();


                var _cluster  = await _cache.GetClusterInfor();
                pool.Start();

                var nodes = _cluster.WorkerNodes;
                var initState = new Dictionary<int,string>();
                nodes.ForEach(x => initState.Add(x.ID,WorkerState.Disconnected));
                await _cache.SetClusterState(initState);

                if(await _infor.IsRegistered()) 
                { 
                    await conductor.Start(); 
                    await _Port.Start();
                }
            }
        }
    }
}
