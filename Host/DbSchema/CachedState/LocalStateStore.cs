using Microsoft.Extensions.Caching.Distributed;
using SharedHost.Models.Device;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DbSchema.LocalDb.Models;
using DbSchema.LocalDb;

namespace DbSchema.CachedState
{
    public interface ILocalStateStore
    {
        Task SetWorkerState(int WorkerID, string? node);

        Task<string?> GetWorkerState(int WorkerID);

        Task<Dictionary<int, string>?> GetClusterState();

        Task<ClusterWorkerNode?> GetWorkerInfor(int PrivateID);

        Task CacheWorkerInfor(ClusterWorkerNode Worker);

        Task SetWorkerRemoteToken(int WorkerID, string token);

        Task<string> GetWorkerRemoteToken (int WorkerID);
    }
    public class LocalStateStore : ILocalStateStore
    {
        private readonly ClusterDbContext _db;
        private IDistributedCache _cache;

        public LocalStateStore(IDistributedCache cache, ClusterDbContext db)
        {
            _cache = cache;
            _db = db;
        }

        public async Task SetWorkerState(int WorkerID, string? node)
        {
            var cachedValue = await _cache.GetRecordAsync<Dictionary<int, string>>("ClusterWorkerCache");
            cachedValue.Remove(WorkerID);
            if(node != null)
            {
                cachedValue.Add(WorkerID, node);
            }
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
            var worker = await _cache.GetRecordAsync<ClusterWorkerNode?>("Worker_" + PrivateID.ToString());
            if(worker == null)
            {
                worker = _db.Devices.Find(PrivateID);
                await CacheWorkerInfor(worker);
            }
            return worker;
        }

        public async Task SetWorkerRemoteToken(int WorkerID, string token)
        {
            Serilog.Log.Information("Caching remote token "+ token + " from WorkerNode: " + WorkerID);
            await _cache.SetRecordAsync<string>("Worker_Session_Token_" + WorkerID.ToString(), token, null,null);
        }

        public async Task<string> GetWorkerRemoteToken (int WorkerID)
        {
            var cachedValue = await _cache.GetRecordAsync<string>("Worker_Session_Token_" + WorkerID.ToString());
            Serilog.Log.Information("Getting remote token "+ cachedValue + " from WorkerNode: " + WorkerID);
            return cachedValue;
        }
    }
}
