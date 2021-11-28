using Microsoft.Extensions.Caching.Distributed;
using SharedHost;
using SharedHost.Models.Cluster;
using SharedHost.Models.Device;
using SharedHost.Models.Session;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DbSchema.CachedState
{
    public interface IGlobalStateStore
    {
        Task SetWorkerState(Dictionary<int, string> node);
        Task<Dictionary<int, string>> GetWorkerState();
        Task SetUserSetting(int SettingID, UserSetting defaultSetting);
        Task<UserSetting> GetUserSetting(int WorkerID);
        Task SetSessionSetting(int SessionID, UserSetting defaultSetting, SystemConfig config, GlobalCluster cluster);
        Task<SessionWorker> GetClientSessionSetting(SessionAccession accession);
        Task<SessionClient> GetWorkerSessionSetting(SessionAccession accession);
        Task<string> GetWorkerStateWithID(int ID);
        Task UpdateWorkerState(int ID, string NewState);
    }



    public class GlobalStateStore : IGlobalStateStore
    {
        private IDistributedCache _cache;

        public GlobalStateStore(IDistributedCache cache)
        {
            _cache = cache;
        }






        public async Task SetWorkerState(Dictionary<int,string> node)
        {
            await _cache.SetRecordAsync<Dictionary<int, string>>("GlobalState", node,TimeSpan.MaxValue,TimeSpan.MaxValue);
        }
        public async Task<Dictionary<int, string>> GetWorkerState ()
        {
            return await _cache.GetRecordAsync<Dictionary<int, string>>("GlobalState");
        }
        public async Task UpdateWorkerState(int ID, string NewState)
        {
            var workerState = await GetWorkerState();
            workerState.Remove(ID);
            workerState.TryAdd(ID, NewState);
            await SetWorkerState(workerState);
        }
        public async Task<string> GetWorkerStateWithID(int ID)
        {
            var workerState = await GetWorkerState();
            workerState.TryGetValue(ID, out var result);
            return result;
        }







        public async Task SetUserSetting(int SettingID, UserSetting defaultSetting)
        {
            await _cache.SetRecordAsync<UserSetting>(SettingID.ToString(), defaultSetting, TimeSpan.FromDays(7), TimeSpan.FromDays(7));
        }

        public async Task<UserSetting> GetUserSetting(int WorkerID)
        {
            return await _cache.GetRecordAsync<UserSetting>(WorkerID.ToString());
        }











        public async Task SetSessionSetting(int SessionID, UserSetting defaultSetting, SystemConfig config, GlobalCluster cluster)
        {
            var sessionWorker = new SessionWorker
            {
                SignallingUrl = config.SignallingWs,
                turnConnection = "turn://" + cluster.turnUSER + ":" + cluster.turnPASSWORD + "@turn:" + cluster.turnIP + ":3478",
                ScreenHeight = defaultSetting.screenHeight,
                ScreenWidth = defaultSetting.screenWidth,
                AudioCodec = defaultSetting.audioCodec,
                VideoCodec = defaultSetting.videoCodec,
                QoEMode = defaultSetting.mode
            };
            var sessionClient = new SessionClient
            {
                SignallingUrl = config.SignallingWs,
                turnIP =  "turn:"+cluster.turnIP + ":3478",
                turnUser = cluster.turnUSER,
                turnPassword = cluster.turnPASSWORD,
                AudioCodec = defaultSetting.audioCodec,
                VideoCodec = defaultSetting.videoCodec,
            };

            config.STUNlist.ForEach(x => sessionClient.STUNlist.Add(x)) ;
            config.STUNlist.ForEach(x => sessionWorker.STUNlist.Add(x));

            await _cache.SetRecordAsync<SessionClient>(SessionID.ToString() + "_" + Module.CLIENT_MODULE.ToString(), sessionClient, TimeSpan.FromDays(7), TimeSpan.FromDays(7));
            await _cache.SetRecordAsync<SessionWorker>(SessionID.ToString() + "_" + Module.CORE_MODULE.ToString(), sessionWorker, TimeSpan.FromDays(7), TimeSpan.FromDays(7));
        }

        public async Task<SessionWorker> GetClientSessionSetting(SessionAccession accession)
        {
            return await _cache.GetRecordAsync<SessionWorker>(accession.ID.ToString() + "_" + accession.Module.ToString());
        }

        public async Task<SessionClient> GetWorkerSessionSetting(SessionAccession accession)
        {
            return await _cache.GetRecordAsync<SessionClient>(accession.ID.ToString() + "_" + accession.Module.ToString());
        }
    }
}
