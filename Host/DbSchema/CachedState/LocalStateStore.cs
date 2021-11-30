using Microsoft.Extensions.Caching.Distributed;
using DbSchema.SystemDb;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SharedHost;
using SharedHost.Models.Device;
using SharedHost.Models.Local;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace DbSchema.CachedState
{
    public interface ILocalStateStore
    {
        Task SetWorkerState(int WorkerID, string node);

        Task<string?> GetWorkerState(int WorkerID);

        Task<Dictionary<int, string>?> GetClusterState();

        Task<ClusterWorkerNode?> GetWorkerInfor(int PrivateID);

        Task CacheWorkerInfor(ClusterWorkerNode Worker);
    }
    public class LocalStateStore : ILocalStateStore
    {
        private IDistributedCache _cache;

        public LocalStateStore(IDistributedCache cache, ClusterDbContext db)
        {
            _cache = cache;
            var nodes = db.Devices.ToList();
            var initState = new Dictionary<int,string>();
            nodes.ForEach(x => initState.Add(x.ID,WorkerState.Open));
            _cache.SetRecordAsync<Dictionary<int,string>>("ClusterWorkerCache", initState, null,null).Wait();
        }

        public async Task SetWorkerState(int WorkerID, string node)
        {
            var cachedValue = await _cache.GetRecordAsync<Dictionary<int, string>>("ClusterWorkerCache");
            cachedValue.Remove(WorkerID);
            cachedValue.Add(WorkerID, node);
            await _cache.SetRecordAsync<Dictionary<int,string>>("ClusterWorkerCache", cachedValue, null,null);
            return;
        }
        public async Task<string?> GetWorkerState(int WorkerID)
        {
            var cachedValue = await _cache.GetRecordAsync<Dictionary<int, string>>("ClusterWorkerCache");
            bool success = cachedValue.TryGetValue(WorkerID, out var result);
            return success ? result : null;
        }
        public async Task<Dictionary<int, string>?> GetClusterState()
        {
            var cachedValue = await _cache.GetRecordAsync<Dictionary<int, string>>("ClusterWorkerCache");
            return cachedValue;
        }

        public async Task CacheWorkerInfor(ClusterWorkerNode Worker)
        {
            await _cache.SetRecordAsync<ClusterWorkerNode>("Worker_" + Worker.ID.ToString(), Worker, null,null);
        }
        public async Task<ClusterWorkerNode?> GetWorkerInfor(int PrivateID)
        {
            var cachedValue = await _cache.GetRecordAsync<ClusterWorkerNode?>("Worker_" + PrivateID.ToString());
            if (cachedValue != null)
            {
                return cachedValue;
            }
            else
            {
                return null;
            }
        }
    }
}
