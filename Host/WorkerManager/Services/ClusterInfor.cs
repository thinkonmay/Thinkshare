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

        public async Task<bool> IsPrivate()
        {
            var cluster = await _cache.GetClusterInfor();

            var request = new RestRequest(_config.ClusterInforUrl, Method.GET)
                .AddHeader("Authorization",cluster.ClusterToken);
            var instanceResult = (await (new RestClient()).ExecuteAsync(request));

            var instance =  JsonConvert.DeserializeObject<GlobalCluster>(instanceResult.Content);
            return instance.Private;
        }

        public async Task<bool> IsSelfHost()
        {
            var cluster = await _cache.GetClusterInfor();

            var request = new RestRequest(_config.ClusterInforUrl, Method.GET)
                .AddHeader("Authorization",cluster.ClusterToken);
            var instanceResult = (await (new RestClient()).ExecuteAsync(request));

            var instance =  JsonConvert.DeserializeObject<GlobalCluster>(instanceResult.Content);
            return instance.SelfHost;
        }

        public async Task<List<PortForward>> PortForward()
        {
            var cluster = await _cache.GetClusterInfor();

            var request = new RestRequest(_config.ClusterInforUrl, Method.GET)
                .AddHeader("Authorization",cluster.ClusterToken);
            var instanceResult = (await (new RestClient()).ExecuteAsync(request));

            var instance =  JsonConvert.DeserializeObject<GlobalCluster>(instanceResult.Content);
            return instance.instance.portForwards;
        }
    }
}