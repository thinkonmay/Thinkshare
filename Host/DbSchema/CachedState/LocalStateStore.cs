using Microsoft.Extensions.Caching.Distributed;
using SharedHost.Models.Cluster;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DbSchema.LocalDb.Models;
using SharedHost.Models.Shell;
using DbSchema.LocalDb;
using System;

namespace DbSchema.CachedState
{
    public interface ILocalStateStore
    {
        Task SetWorkerState(int WorkerID, string? node);

        Task<string?> GetWorkerState(int WorkerID);

        Task<Dictionary<int, string>?> GetClusterState();

        Task<Dictionary<int, string>?> SetClusterState(Dictionary<int, string> state);

        Task<ClusterWorkerNode?> GetWorkerInfor(int PrivateID);

        Task CacheWorkerInfor(ClusterWorkerNode Worker);


        Task<ClusterCredential?> GetClusterInfor();

        Task CacheClusterInfor(ClusterCredential Worker);


        Task SetWorkerRemoteToken(int WorkerID, string token);

        Task<string> GetWorkerRemoteToken (int WorkerID);


        Task CacheScriptModel(List<ScriptModel> models);

        Task<List<ScriptModel>> GetScriptModel();

        Task Log(int WorkerID, Log token);

        Task<Log> GetLog(int WorkerID, DateTime? Start, DateTime? End);
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
        public async Task SetClusterState(Dictionary<int,string> state)
        {
            await _cache.SetRecordAsync<Dictionary<int, string>>("ClusterWorkerCache",state,null,null);
        }





        public async Task CacheWorkerInfor(ClusterWorkerNode Worker)
        {
            var cluster = await GetClusterInfor();
            foreach (var item in cluster.WorkerNodes)
            {
                if(item.ID == Worker.ID)
                {
                    cluster.WorkerNodes.Remove(item);
                }
            }

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








        public async Task SetClusterInfor(ClusterCredential cred)
        {
            await _cache.SetRecordAsync<ClusterCredential>("Cluster_Infor", cred, null,null);
        }

        public async Task<ClusterCredential> GetClusterInfor()
        {
            var cachedValue = await _cache.GetRecordAsync<ClusterCredential>("Cluster_Infor");
            return cachedValue;
        }








        public async Task CacheShellSession(int WorkerID, ShellSession token)
        {
            var sessions = await _cache.GetRecordAsync<List<ShellSession>>("Shell_Session_" + WorkerID.ToString());
            sessions.Add(token);
            await _cache.SetRecordAsync<List<ShellSession>>("Shell_Session_" + WorkerID.ToString(), sessions, null,null);
        }

        public async Task<ShellSession> GetCachedShellSession(int WorkerID)
        {
            var cachedValue = await _cache.GetRecordAsync<ShellSession>("Shell_Session_" + WorkerID.ToString());
            return cachedValue;
        }





        public async Task Log(Log log, int WorkerID)
        {
            var sessions = await _cache.GetRecordAsync<List<Log>>("Shell_Session_" + WorkerID.ToString());
            sessions.Add(log);
            await _cache.SetRecordAsync<List<Log>>("Shell_Session_" + WorkerID.ToString(), sessions, null,null);
        }

        public async Task<Log> GetLog(int WorkerID, DateTime? Start, DateTime? End)
        {
            var cachedValue = await _cache.GetRecordAsync<Log>("Log_"+Log.ID);
            if(Start != null && End != null)
            {

            }

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
}
