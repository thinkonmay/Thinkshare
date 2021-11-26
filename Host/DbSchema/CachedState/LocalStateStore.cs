using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using SharedHost.Models.Device;
using SharedHost.Models.Local;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DbSchema.CachedState
{
    public interface ILocalStateStore
    {
        Task SetWorkerState(int WorkerID, string node);

        Task<string> GetWorkerState(int WorkerID);

        Task<Dictionary<int, string>> GetClusterState();

        Task<ClusterWorkerNode?> GetWorkerInfor(int PrivateID);

        Task CacheWorkerInfor(ClusterWorkerNode Worker);
    }
    public class LocalStateStore : ILocalStateStore
    {
        private IDistributedCache _cache;

        public LocalStateStore(IDistributedCache cache)
        {
            _cache = cache;
        }
        public async Task SetWorkerState(int WorkerID, string node)
        {
            var cachedValue = await _cache.GetRecordAsync<string>("ClusterWorkerCache");
            var dictionary = JsonConvert.DeserializeObject<Dictionary<int, string>>(cachedValue);
            dictionary.Remove(WorkerID);
            dictionary.Add(WorkerID, node);
            await _cache.SetRecordAsync<string>("ClusterWorkerCache", node, TimeSpan.MaxValue, TimeSpan.MaxValue);
            return;
        }
        public async Task<string> GetWorkerState(int WorkerID)
        {
            var cachedValue = await _cache.GetRecordAsync<string>("ClusterWorkerCache");
            JsonConvert.DeserializeObject<Dictionary<int, string>>(cachedValue).TryGetValue(WorkerID, out var result);
            return result;
        }
        public async Task<Dictionary<int, string>> GetClusterState()
        {
            var cachedValue = await _cache.GetRecordAsync<string>("ClusterWorkerCache");
            var result = JsonConvert.DeserializeObject<Dictionary<int, string>>(cachedValue); ;
            return result;
        }

        public async Task CacheWorkerInfor(ClusterWorkerNode Worker)
        {
            await _cache.SetRecordAsync<ClusterWorkerNode>("Worker_" + Worker.PrivateID.ToString(), Worker, TimeSpan.FromDays(1), TimeSpan.FromDays(1));
        }
        public async Task<ClusterWorkerNode?> GetWorkerInfor(int PrivateID)
        {
            var cachedValue = await _cache.GetRecordAsync<string>("Worker_" + PrivateID.ToString());
            if (cachedValue != null)
            {
                return JsonConvert.DeserializeObject<ClusterWorkerNode>(cachedValue);
            }
            else
            {
                return null;
            }
        }
    }
}
