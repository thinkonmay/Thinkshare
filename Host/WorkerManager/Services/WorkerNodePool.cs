using System;
using System.Threading;
using System.Linq;
using WorkerManager.Interfaces;
using SharedHost.Models.Device;
using System.Threading.Tasks;

using WorkerManager.Models;

namespace WorkerManager.Services
{
    public class WorkerNodePool : IWorkerNodePool
    {
        private readonly ILocalStateStore _cache;

        private Task _stateStyncing;
        
        private Task _systemHeartBeat;

        private Task _workerShell;

        private bool isRunning;

        public WorkerNodePool(ILocalStateStore cache)
        {
            _cache = cache;
            isRunning = false;
        }

        public bool Start()
        {
            if(!isRunning)
            {
                isRunning = true;
                _systemHeartBeat =  Task.Run(() => SystemHeartBeat());
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
                _workerShell.Wait();
                _systemHeartBeat.Wait();
                return true;
            }
            else
            {
                return false;
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
            var model_list = await _cache.GetScriptModel();
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
                    worker.GetWorkerMetric(_cache,model_list);
                }
                Thread.Sleep(((int)TimeSpan.FromSeconds(60).TotalMilliseconds));
            }
        }

        async Task PushCachedShellSession()
        {
            DateTime currentTime = DateTime.Now;
            while (true)
            {
                Thread.Sleep((int)TimeSpan.FromDays(1).TotalMilliseconds);
                currentTime.AddDays(1);
            }
        }
    }
}
