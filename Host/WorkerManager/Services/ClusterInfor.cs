using RestSharp;
using SharedHost.Models.AWS;
using System.Collections.Generic;
using SharedHost;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SharedHost.Models.Cluster;
using WorkerManager.Interfaces;

namespace WorkerManager.Services
{
    public class ClusterInfor : IClusterInfor
    {
        private readonly ClusterConfig _config;

        private readonly ILocalStateStore _cache;
        
        public ClusterInfor(IOptions<ClusterConfig> config,
                            ILocalStateStore cache)
        {
            _config = config.Value;
            _cache = cache;
        }

        public async Task<bool> IsRegistered()
        {
            return ((await _cache.GetClusterInfor()).ClusterToken != null);
        }

        public async Task<GlobalCluster> Infor()
        {
            var cluster = await _cache.GetClusterInfor();

            var request = new RestRequest($"https://{_config.Domain}{_config.ClusterInforUrl}",Method.GET)
                .AddHeader("Authorization",cluster.ClusterToken);

            var result = await (new RestClient()).ExecuteAsync(request);
            return JsonConvert.DeserializeObject<GlobalCluster>(result.Content);
        }
    }
}