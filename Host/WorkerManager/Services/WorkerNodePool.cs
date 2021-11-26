using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using WorkerManager.Interfaces;
using SharedHost.Models.Shell;
using SharedHost.Models.Device;
using System.Threading.Tasks;
using DbSchema.CachedState;
using WorkerManager.Data;
using SharedHost.Models.Local;

namespace WorkerManager.Services
{
    public class WorkerNodePool 
    {
        private readonly ConductorSocket _socket;

        private readonly ILocalStateStore _cache;

        private readonly ClusterDbContext _db;

        public WorkerNodePool(ConductorSocket socket, 
                              ILocalStateStore cache,
                              ClusterDbContext db)
        {
            _cache = cache;
            _socket = socket;
            _db = db;
            Task.Run(() => SystemHeartBeat());
            Task.Run(() => StateSyncing());
            Task.Run(() => SessionHeartBeat());
        }

        public async Task SessionHeartBeat()
        {
            try
            {
                while(true)
                {
                    var worker_list = await _cache.GetClusterState();
                    foreach (var item in worker_list)
                    {
                        if(item.Value != WorkerState.OnSession)
                        {
                            continue;
                        }

                        // find is cache first, the find in sqldb if not present on redis
                        ClusterWorkerNode worker = await _cache.GetWorkerInfor(item.Key);
                        if (worker == null)
                        {
                            worker = _db.Devices.Find(item.Key);
                            await _cache.CacheWorkerInfor(worker);
                        }

                        worker.RestoreWorkerNode();
                        var result = await worker.PingSession();
                        if(result)
                        {
                            worker.sessionFailedPing = 0;
                            continue;
                        }
                        else
                        {
                            worker.sessionFailedPing++;
                        }

                        if(worker.sessionFailedPing > 5)
                        {
                            if (item.Value == WorkerState.OnSession)
                            {
                                await _cache.SetWorkerState(item.Key, WorkerState.OffRemote);
                            }
                        }
                    }
                    Thread.Sleep(((int)TimeSpan.FromSeconds(1).TotalMilliseconds));
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Information("ping session failed due to " + ex.Message);
                Thread.Sleep(((int)TimeSpan.FromSeconds(1).TotalMilliseconds));
                await SessionHeartBeat();
            }
        }

        public async Task SystemHeartBeat()
        {
            var _model_list = await _socket.GetDefaultModel();
            try
            {
                while(true)
                {
                    var worker_list = await _cache.GetClusterState();
                    foreach (var i in _model_list)
                    {
                        foreach (var keyValue in worker_list)
                        {
                            ClusterWorkerNode worker = await _cache.GetWorkerInfor(keyValue.Key);

                            if(worker == null)
                            {
                                worker = _db.Devices.Find(keyValue.Key);
                                await _cache.CacheWorkerInfor(worker);
                            }
                            worker.RestoreWorkerNode();
                            var session = new ShellSession { Script = i.Script };
                            var result = await worker.PingWorker(session);
                            if(result != null)
                            {
                                session.Output = result;
                                session.ModelID = i.ID;
                                session.WorkerID = worker.GlobalID;
                                session.Time = DateTime.Now;
                                worker.agentFailedPing = 0;
                            }
                            else
                            {
                                worker.agentFailedPing++;
                            }

                            if(worker.agentFailedPing > 5)
                            {
                                await _cache.SetWorkerState(keyValue.Key, WorkerState.OffRemote);
                            }
                            if(session != null)
                            {
                                _db.CachedSession.Add(session);
                                await _db.SaveChangesAsync();
                            }
                        }
                    }
                    Thread.Sleep(((int)TimeSpan.FromSeconds(10).TotalMilliseconds));
                }
            }catch (Exception ex)
            {
                Serilog.Log.Information("ping worker failed due to " + ex.Message);
                Thread.Sleep(((int)TimeSpan.FromSeconds(10).TotalMilliseconds));
                await SystemHeartBeat();
            }
        }

        public async Task StateSyncing()
        {
            try
            {
                while (true)
                {
                    var worker_list = await _cache.GetClusterState();
                    foreach ( var unsyncedDevice in worker_list)
                    {
                        await _socket.WorkerStateSyncing(unsyncedDevice.Key, unsyncedDevice.Value);
                    }
                    Thread.Sleep(((int)TimeSpan.FromMilliseconds(100).TotalMilliseconds));
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Information("state syncing failed due to " + ex.Message);
                Thread.Sleep(((int)TimeSpan.FromSeconds(1).TotalMilliseconds));
                await StateSyncing();
            }
        }
    }
}
