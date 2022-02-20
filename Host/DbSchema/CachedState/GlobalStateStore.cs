using Microsoft.Extensions.Caching.Distributed;
using System;
using Microsoft.Extensions.Options;
using RestSharp;
using System.Linq;
using DbSchema.SystemDb.Data;
using SharedHost;
using Newtonsoft.Json;
using SharedHost.Models.Cluster;
using SharedHost.Models.Device;
using SharedHost.Models.Session;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedHost.Logging;

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

        Task<SessionClient> GetClientSessionSetting(SessionAccession accession);
        Task<SessionWorker> GetWorkerSessionSetting(SessionAccession accession);
        Task<string?> GetWorkerState(int WorkerID);
    }



    public class GlobalStateStore : IGlobalStateStore
    {
        private IDistributedCache _cache;

        private GlobalDbContext _db;

        private readonly SystemConfig _config;

        private readonly ILog _log;

        public GlobalStateStore(IDistributedCache cache,
                                IOptions<SystemConfig> config,
                                ILog log,
                                GlobalDbContext db)
        {
            _db = db;
            _log = log;
            _cache = cache;
            _config = config.Value;
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
                await SetClusterSnapshot(ClusterID,snapshoot);
            }
            return snapshoot;
        }

        public async Task<string?> GetWorkerState(int WorkerID)
        {
            var worker = _db.Devices.Find(WorkerID);

            if(worker == null)
                return null;

            var cluster = _db.Clusters.Where(x => 
                    !x.Unregister.HasValue && 
                     x.WorkerNode.Contains(worker)).First();

            var snapshoot = await this.GetClusterSnapshot(cluster.ID);
            snapshoot.TryGetValue(WorkerID,out var state);
            return state;
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











        public async Task<SessionClient> GetClientSessionSetting(SessionAccession accession)
        {
            var worker = _db.Devices.Find(accession.WorkerID);
            var cluster = _db.Clusters.Where(x => !x.Unregister.HasValue && 
                                                   x.WorkerNode.Contains(worker)).First();

            var setting = await GetUserSetting(accession.ClientID);
            
            if(cluster == null)
                throw new Exception("Fail to get worker setting");

            var sessionClient = new SessionClient
            {
                stuns =         _config.STUNlist,
                signallingurl = _config.SignallingWs,

                turn =  $"turn://{cluster.instance.TurnUser}:{cluster.instance.TurnPassword}@{cluster.instance.IPAdress}:3478",
                turnip =  $"turn:{cluster.instance.IPAdress}:3478",
                turnuser =        cluster.instance.TurnUser,
                turnpassword =    cluster.instance.TurnPassword,

                audiocodec = setting.audioCodec,
                videocodec = setting.videoCodec,
            };

            _log.Information("Got client session in cache :"+ JsonConvert.SerializeObject(sessionClient));
            return sessionClient;
        }

        public async Task<SessionWorker> GetWorkerSessionSetting(SessionAccession accession)
        {
            var worker = _db.Devices.Find(accession.WorkerID);
            var cluster = _db.Clusters.Where(x => !x.Unregister.HasValue && 
                                                   x.WorkerNode.Contains(worker)).First();
            var setting = await GetUserSetting(accession.ClientID);

            if(cluster == null)
                throw new Exception("Invalid worker");

            var sessionWorker = new SessionWorker
            {
                stuns =         _config.STUNlist,
                signallingurl = _config.SignallingWs,

                turn = $"turn://{cluster.instance.TurnUser}:{cluster.instance.TurnPassword}@{cluster.instance.IPAdress}:3478",

                clientdevice =          setting.device,
                clientengine =          setting.engine,
                screenheight =          setting.screenHeight,
                screenwidth =           setting.screenWidth,
                audiocodec =            setting.audioCodec,
                videocodec =            setting.videoCodec,
                mode =                  setting.mode,
            };

            _log.Information("Got worker session in cache :"+ JsonConvert.SerializeObject(sessionWorker));
            return sessionWorker;
        }
    }
}
