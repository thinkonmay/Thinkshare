using System;
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

        private Task _workerShell;

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
                _workerShell =      Task.Run(() => GetWorkerMetric());
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
                    foreach (var item in worker_list)
                    {
                        var workerState = await _cache.GetWorkerState(item.Key);
                        if(workerState == WorkerState.OnSession)
                        {
                            // find is cache first, the find in sqldb if not present on redis
                            ClusterWorkerNode worker = await _cache.GetWorkerInfor(item.Key);
                            worker.RestoreWorkerNode();
                            if(await worker.PingWorker(Module.CORE_MODULE))
                            {
                                worker.sessionFailedPing = 0;
                            }
                            else
                            {
                                worker.sessionFailedPing++;
                            }
                            await _cache.CacheWorkerInfor(worker);
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
            try
            {
                while(true)
                {
                    if(!isRunning)
                    {
                        return;
                    }

                    var worker_list = await _cache.GetClusterState();
                    foreach (var keyValue in worker_list)
                    {
                        ClusterWorkerNode worker = await _cache.GetWorkerInfor(keyValue.Key);
                        worker.RestoreWorkerNode();
                        if(await worker.PingWorker(Module.AGENT_MODULE))
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





                        if(worker.agentFailedPing > 10)
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

        async Task GetWorkerMetric()
        {
            var model_list = _db.ScriptModels.ToList();
            while (true)
            {
                if(!isRunning)
                {
                    return;
                }
                var worker_list = await _cache.GetClusterState();
                foreach (var item in worker_list.Where(x => x.Value != WorkerState.Disconnected))
                {
                    ClusterWorkerNode worker = await _cache.GetWorkerInfor(item.Key);
                    worker.RestoreWorkerNode();
                    worker.GetWorkerMetric(_db,model_list);
                }
                Thread.Sleep(((int)TimeSpan.FromSeconds(60).TotalMilliseconds));
            }
        }

        async Task PushCachedShellSession()
        {
            DateTime currentTime = DateTime.Now;
            while (true)
            {
                var CachedSession = _db.CachedSession.All(x => true);

                Thread.Sleep((int)TimeSpan.FromDays(1).TotalMilliseconds);
                currentTime.AddDays(1);
            }
        }

    }
}
