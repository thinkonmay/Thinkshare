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

        private readonly ILog _log;

        public WorkerNodePool(ILocalStateStore cache,
                              ILog log)
        {
            _log = log;
            _cache = cache;
            isRunning = false;
        }

        public bool Start()
        {
            if(isRunning)
                return false;

            isRunning = true;
            _systemHeartBeat =  Task.Run(() => SystemHeartBeat());
            _workerShell =      Task.Run(() => GetWorkerMetric());
            return true;
        }

        public bool Stop()
        {
            if(!isRunning)
                return false;

            isRunning = false;
            _workerShell.Wait();
            _systemHeartBeat.Wait();
            return true;
        }


        public async Task SystemHeartBeat()
        {
            while(isRunning)
            {
                try
                {
                    var workers = await _cache.GetClusterState();
                    foreach (var keyValue in workers)
                    {
                        var worker = await _cache.GetWorkerInfor(keyValue.Key);
                        var success = await worker.PingWorker();

                        worker.agentFailedPing = success ? 0 : (worker.agentFailedPing + 1);
                        await _cache.CacheWorkerInfor(worker);

                        string newState = null;
                        newState = ((keyValue.Value == WorkerState.Disconnected) && success) ? WorkerState.Open : null;
                        newState = (worker.agentFailedPing > 3) ? WorkerState.Disconnected : null;

                        if(newState != null)
                            await _cache.SetWorkerState(keyValue.Key, newState);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("ping worker failed",ex);
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        async Task GetWorkerMetric()
        {
            var models = await _cache.GetScriptModel();
            while(isRunning)
            {
                var workers = await _cache.GetClusterState();
                foreach (var worker in workers)
                {
                    if(worker.Value == WorkerState.Disconnected)
                        continue;

                    foreach (var model in models)
                    {
                        var infor = await _cache.GetWorkerInfor(worker.Key);
                        var res = await infor.Execute(model.Script);
                        _log.Worker(res.Content,infor.ID.ToString(),model.ID.ToString());
                    }
                }
                Thread.Sleep(TimeSpan.FromSeconds(60));
            }
        }
    }
}