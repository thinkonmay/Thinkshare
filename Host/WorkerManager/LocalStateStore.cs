using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using WorkerManager.Models;
using SharedHost.Models.Shell;
using Newtonsoft.Json;
using System;

namespace WorkerManager
{
    public interface ILocalStateStore
    {
        Task SetWorkerState(int WorkerID, string? node);

        Task<string?> GetWorkerState(int WorkerID);

        Task<Dictionary<int, string>?> GetClusterState();

        Task SetClusterState(Dictionary<int, string> state);

        Task<ClusterWorkerNode?> GetWorkerInfor(int PrivateID);

        Task CacheWorkerInfor(ClusterWorkerNode Worker);


        Task CacheShellSession(int WorkerID, ShellSession token);

        Task<ShellSession> GetCachedShellSession(int WorkerID);

        Task<ClusterKey?> GetClusterInfor();

        Task SetClusterInfor(ClusterKey Worker);


        Task SetWorkerRemoteToken(int WorkerID, string token);

        Task<string> GetWorkerRemoteToken (int WorkerID);


        Task CacheScriptModel(List<ScriptModel> models);

        Task<List<ScriptModel>> GetScriptModel();

        Task Log(int WorkerID, Log token);

        Task<List<Log>> GetLog(int WorkerID, DateTime? Start, DateTime? End);
    }






    public class LocalStateStore : ILocalStateStore
    {
        private IDistributedCache _cache;

        public LocalStateStore(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task SetWorkerState(int WorkerID, string? node)
        {
            var cachedValue = await _cache.GetRecordAsync<Dictionary<int, string>>("CLUSTER_STATE_CACHE");
            cachedValue.Remove(WorkerID);
            if(node != null)
            {
                cachedValue.Add(WorkerID, node);
            }
            await _cache.SetRecordAsync<Dictionary<int,string>>("CLUSTER_STATE_CACHE", cachedValue, null,null);
            return;
        }
        public async Task<string?> GetWorkerState(int WorkerID)
        {
            var cachedValue = await _cache.GetRecordAsync<Dictionary<int, string>>("CLUSTER_STATE_CACHE");
            bool success = cachedValue.TryGetValue(WorkerID, out var result);
            return success ? result : null;
        }
        public async Task<Dictionary<int, string>?> GetClusterState()
        {
            var cachedValue = await _cache.GetRecordAsync<Dictionary<int, string>>("CLUSTER_STATE_CACHE");
            return cachedValue;
        }
        public async Task SetClusterState(Dictionary<int,string> state)
        {
            await _cache.SetRecordAsync<Dictionary<int, string>>("CLUSTER_STATE_CACHE",state,null,null);
        }





        public async Task CacheWorkerInfor(ClusterWorkerNode Worker)
        {
            var cluster = await GetClusterInfor();
            cluster.WorkerNodes.RemoveAll(x => x.ID == Worker.ID);
            cluster.WorkerNodes.Add(Worker);
            await SetClusterInfor(cluster);
            return;
        }
        public async Task<ClusterWorkerNode?> GetWorkerInfor(int ID)
        {
            var cluster = await GetClusterInfor();
            return cluster.WorkerNodes.Where(x => x.ID == ID).First();
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








        public async Task SetClusterInfor(ClusterKey cred)
        {
            await _cache.SetRecordAsync<ClusterKey>("Cluster_Infor", cred, null,null);
        }

        public async Task<ClusterKey> GetClusterInfor()
        {
            var cachedValue = await _cache.GetRecordAsync<ClusterKey>("Cluster_Infor");
            return cachedValue;
        }








        public async Task CacheShellSession(int WorkerID, ShellSession session)
        {
            session.Time = DateTime.Now;
            var sessions = await _cache.GetRecordAsync<List<ShellSession>>("Shell_Session_" + WorkerID.ToString());
            sessions.Add(session);
            await _cache.SetRecordAsync<List<ShellSession>>("Shell_Session_" + WorkerID.ToString(), sessions, null,null);
        }

        public async Task<ShellSession> GetCachedShellSession(int WorkerID)
        {
            var cachedValue = await _cache.GetRecordAsync<ShellSession>("Shell_Session_" + WorkerID.ToString());
            return cachedValue;
        }





        public async Task Log(int WorkerID,Log log)
        {
            var sessions = await _cache.GetRecordAsync<List<Log>>("Log_" + WorkerID.ToString()+log.LogTime.DayOfYear+log.LogTime.Hour);
            sessions.Add(log);
            await _cache.SetRecordAsync<List<Log>>("Log_" + WorkerID.ToString()+log.LogTime.Day, sessions, null,null);
        }

        public async Task<List<Log>> GetLog(int WorkerID, DateTime? Start, DateTime? End)
        {
            var cachedValue = await _cache.GetRecordAsync<List<Log>>("Log_"+WorkerID.ToString()+Start.Value.DayOfYear+Start.Value.Hour);
            return cachedValue;
        }






        public async Task CacheScriptModel(List<ScriptModel> models)
        {
            await _cache.SetRecordAsync<List<ScriptModel>>("Script_Model", models, null,null);
        }

        public async Task<List<ScriptModel>> GetScriptModel()
        {
            return await _cache.GetRecordAsync<List<ScriptModel>>("Script_Model");
        }
    }

    public static class DistributedCacheExtensions
    {
        public static async Task SetRecordAsync<T>(this IDistributedCache cache,
            string recordId,
            T data,
            TimeSpan? absoluteExpireTime = null,
            TimeSpan? unusedExpireTime = null)
        {
            var options = new DistributedCacheEntryOptions();

            options.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30);
            options.SlidingExpiration = TimeSpan.FromDays(30);

            var jsonData = JsonConvert.SerializeObject(data);
            await cache.SetStringAsync(recordId, jsonData, options);
        }

        public static async Task<T> GetRecordAsync<T>(this IDistributedCache cache, string recordId)
        {
            var jsonData = await cache.GetStringAsync(recordId);

            if (jsonData is null)
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(jsonData);
        }
    }
}
