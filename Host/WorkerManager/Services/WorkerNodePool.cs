﻿using System;
using System.Threading;
using System.Linq;
using WorkerManager.Interfaces;
using SharedHost.Models.Device;
using System.Threading.Tasks;
using DbSchema.CachedState;
using DbSchema.LocalDb;
using DbSchema.LocalDb.Models;

namespace WorkerManager.Services
{
    public class WorkerNodePool : IWorkerNodePool
    {
        private readonly IConductorSocket _socket;

        private readonly ILocalStateStore _cache;

        private readonly ClusterDbContext _db;
        
        private Task _stateStyncing;
        
        private Task _systemHeartBeat;

        private Task _sessionHeartBeat;

        private bool isRunning;

        public WorkerNodePool(IConductorSocket socket, 
                              ILocalStateStore cache,
                              ClusterDbContext db)
        {
            _cache = cache;
            _socket = socket;
            isRunning = false;
            _db = db;
        }

        public bool Start()
        {
            if(!isRunning)
            {
                isRunning = true;
                _systemHeartBeat =  Task.Run(() => SystemHeartBeat());
                _sessionHeartBeat = Task.Run(() => SessionHeartBeat());
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Stop()
        {
            if(isRunning)
            {
                isRunning = false;
                _systemHeartBeat.Wait();
                _sessionHeartBeat.Wait();
                return true;
            }
            else
            {
                return false;
            }
        }


        public async Task SessionHeartBeat()
        {
            try
            {
                while(true)
                {
                    if(!isRunning)
                    {
                        return;
                    }
                    
                    var worker_list = await _cache.GetClusterState();
                    if(worker_list == null)
                    {
                        Thread.Sleep(((int)TimeSpan.FromSeconds(10).TotalMilliseconds));
                        continue;

                    }

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
                Serilog.Log.Information("ping session failed");
                Serilog.Log.Information(ex.Message);
                Serilog.Log.Information(ex.StackTrace);
                Thread.Sleep(((int)TimeSpan.FromSeconds(1).TotalMilliseconds));
                await SessionHeartBeat();
            }
        }

        public async Task SystemHeartBeat()
        {
            var _model_list = _db.ScriptModels.ToList();
            try
            {
                while(true)
                {
                    if(!isRunning)
                    {
                        return;
                    }

                    var worker_list = await _cache.GetClusterState();
                    if(worker_list == null)
                    {
                        Thread.Sleep(((int)TimeSpan.FromSeconds(10).TotalMilliseconds));
                        continue;

                    }
                    foreach (var keyValue in worker_list)
                    {
                        ClusterWorkerNode worker = await _cache.GetWorkerInfor(keyValue.Key);
                        if(worker == null)
                        {
                            worker = _db.Devices.Find(keyValue.Key);
                            await _cache.CacheWorkerInfor(worker);
                        }

                        worker.RestoreWorkerNode();
                        var success = await worker.PingWorker(_db,_model_list);
                        if(success)
                        {
                            worker.agentFailedPing = 0;
                            if(keyValue.Value == WorkerState.Disconnected)
                            {
                                await _cache.SetWorkerState(keyValue.Key, WorkerState.Open);
                            }
                        }
                        else
                        {
                            worker.agentFailedPing++;
                        }
                        await _cache.CacheWorkerInfor(worker);

                        if(worker.agentFailedPing > 5)
                        {
                            await _cache.SetWorkerState(keyValue.Key, WorkerState.Disconnected);
                        }
                    }
                    Thread.Sleep(((int)TimeSpan.FromSeconds(1).TotalMilliseconds));
                }
            }catch (Exception ex)
            {
                Serilog.Log.Information("ping worker failed");
                Serilog.Log.Information(ex.Message);
                Serilog.Log.Information(ex.StackTrace);
                Thread.Sleep(((int)TimeSpan.FromSeconds(10).TotalMilliseconds));
                await SystemHeartBeat();
            }
        }

        async Task PushCachedShellSession()
        {
            DateTime currentTime = DateTime.Now;
            while (true)
            {
                var CachedSession = _db.CachedSession.All(x => true);
                // var client = new RestClient();
                
                // var request = new RestRequest()
                //     .AddJsonBody(CachedSession);

                Thread.Sleep((int)TimeSpan.FromDays(1).TotalMilliseconds);
                currentTime.AddDays(1);
            }
        }

    }
}
