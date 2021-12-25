using Microsoft.Extensions.Caching.Distributed;
using System.Linq;
using DbSchema.SystemDb.Data;
using SharedHost;
using Newtonsoft.Json;
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
        Task SetClusterSnapshot(int ClusterID, Dictionary<int, string> snapshoot);
        Task<Dictionary<int, string>> GetClusterSnapshot(int ClusterID);


        Task CacheWorkerInfor(WorkerNode node);
        Task<WorkerNode?> GetWorkerInfor(int WorkerID);

        Task SetUserSetting(int SettingID, UserSetting defaultSetting);
        Task<UserSetting> GetUserSetting(int WorkerID);

        Task SetSessionSetting(int SessionID, UserSetting defaultSetting, SystemConfig config, GlobalCluster cluster);
        Task<SessionClient> GetClientSessionSetting(SessionAccession accession);
        Task<SessionWorker> GetWorkerSessionSetting(SessionAccession accession);
        Task<string?> GetWorkerState(int WorkerID);
    }



    public class GlobalStateStore : IGlobalStateStore
    {
        private IDistributedCache _cache;

        private GlobalDbContext _db;

        public GlobalStateStore(IDistributedCache cache,
                                GlobalDbContext db)
        {
            _db = db;
            _cache = cache;
        }


        public async Task SetClusterSnapshot(int ClusterID, Dictionary<int, string> snapshoot)
        {
            await _cache.SetRecordAsync<Dictionary<int, string>>("ClusterSnapshoot_"+ClusterID.ToString(), snapshoot,null,null);

        }
        public async Task<Dictionary<int, string>> GetClusterSnapshot(int ClusterID)
        {
            var snapshoot = await _cache.GetRecordAsync<Dictionary<int, string>>("ClusterSnapshoot_"+ClusterID.ToString());
            if(snapshoot == null)
            {
                snapshoot = new Dictionary<int, string>();
                await _cache.SetRecordAsync<Dictionary<int, string>>("ClusterSnapshoot_"+ClusterID.ToString(), snapshoot,null,null);
            }
            return snapshoot;
        }

        public async Task<string?> GetWorkerState(int WorkerID)
        {
            var clusters = _db.Clusters.Where(X => true);
            foreach (var item in clusters)
            {
                var snapshoot = await this.GetClusterSnapshot(item.ID);
                foreach (var state in snapshoot)
                {
                    if (state.Key == WorkerID)
                    {
                        return state.Value;
                    }
                }
            }
            return null;
        }



        public async Task CacheWorkerInfor(WorkerNode node)
        {
            await _cache.SetRecordAsync<WorkerNode>("WorkerInfor_"+node.ID.ToString(), node,null,null);
        }
        public async Task<WorkerNode?> GetWorkerInfor(int WorkerID)
        {
            var result = await _cache.GetRecordAsync<WorkerNode?>("WorkerInfor_"+WorkerID.ToString());
            if (result == null)
            {
                result = _db.Devices.Find(WorkerID);
                await CacheWorkerInfor(result);
            }
            return result;
        }











        public async Task SetUserSetting(int SettingID, UserSetting defaultSetting)
        {
            await _cache.SetRecordAsync<UserSetting>(SettingID.ToString(), defaultSetting, null,null);
        }

        public async Task<UserSetting> GetUserSetting(int UserID)
        {
            var setting = await _cache.GetRecordAsync<UserSetting?>(UserID.ToString());
            if(setting == null)
            {
                setting = new UserSetting {
                    device = DeviceType.WEB_APP,
                    videoCodec = Codec.CODEC_H264,
                    audioCodec = Codec.OPUS_ENC,
                    mode = QoEMode.HIGH_CONST,
                    screenHeight = 1920,
                    screenWidth = 1080                         
                };
                await _cache.SetRecordAsync<UserSetting>(UserID.ToString(), setting);
            }
            return setting;
        }











        public async Task SetSessionSetting(int SessionID, UserSetting defaultSetting, SystemConfig config, GlobalCluster cluster)
        {
            var sessionWorker = new SessionWorker
            {
                signallingurl = config.SignallingWs,
                turn = "turn://" + cluster.TurnUser + ":" + cluster.TurnPassword + "@" + cluster.TurnIp + ":3478",

                clientdevice = defaultSetting.device,
                clientengine = defaultSetting.engine,

                screenheight = defaultSetting.screenHeight,
                screenwidth = defaultSetting.screenWidth,

                audiocodec = defaultSetting.audioCodec,
                videocodec = defaultSetting.videoCodec,

                mode = defaultSetting.mode,
                stuns = config.STUNlist
            };
            var sessionClient = new SessionClient
            {
                signallingurl = config.SignallingWs,
                turn = "turn://" + cluster.TurnUser + ":" + cluster.TurnPassword + "@" + cluster.TurnIp + ":3478",

                turnip =  "turn:"+cluster.TurnIp + ":3478",
                turnuser = cluster.TurnUser,
                turnpassword = cluster.TurnPassword,

                audiocodec = defaultSetting.audioCodec,
                videocodec = defaultSetting.videoCodec,

                stuns = config.STUNlist
            };

            Serilog.Log.Information("setting up session setting for session id "+ SessionID.ToString());
            Serilog.Log.Information("Client session "+ JsonConvert.SerializeObject(sessionClient));
            Serilog.Log.Information("Worker session "+ JsonConvert.SerializeObject(sessionWorker));

            await _cache.SetRecordAsync<SessionClient>(SessionID.ToString() + "_CLIENT_MODULE", sessionClient, null,null);
            await _cache.SetRecordAsync<SessionWorker>(SessionID.ToString() + "_CORE_MODULE", sessionWorker, null,null);
        }

        public async Task<SessionClient> GetClientSessionSetting(SessionAccession accession)
        {
            var result = await _cache.GetRecordAsync<SessionClient>(accession.ID.ToString() + "_CLIENT_MODULE");
            Serilog.Log.Information("Got client session in cache :"+ JsonConvert.SerializeObject(result));
            return result;
        }

        public async Task<SessionWorker> GetWorkerSessionSetting(SessionAccession accession)
        {
            var result = await _cache.GetRecordAsync<SessionWorker>(accession.ID.ToString() + "_CORE_MODULE");
            Serilog.Log.Information("Got worker session in cache :"+ JsonConvert.SerializeObject(result));
            return result;
        }
    }
}
