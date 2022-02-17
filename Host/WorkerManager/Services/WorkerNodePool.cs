using System;
using System.Threading;
using System.Linq;
using WorkerManager.Interfaces;
using SharedHost.Models.Device;
using System.Threading.Tasks;
using WorkerManager.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

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
            // _workerShell =      Task.Run(() => GetWorkerMetric()); // TODO
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
                    var updated = new Dictionary<int,string>();
                    var workers = await _cache.GetClusterState();
                    foreach (var keyValue in workers)
                    {
                        var worker = await _cache.GetWorkerInfor(keyValue.Key);
                        var success = await worker.PingWorker();
                        worker.agentFailedPing = success ? 0 : (worker.agentFailedPing + 1);
                        _log.Worker($"Ping failed {worker.agentFailedPing} time", keyValue.Key.ToString());
                        await _cache.CacheWorkerInfor(worker);
                        bool failMultipletime = (worker.agentFailedPing > 3);
                        bool currentlyDisconnected = (keyValue.Value == WorkerState.Disconnected);


                        var currentState = await _cache.GetWorkerState(keyValue.Key);
                        if(success)
                            updated[keyValue.Key] = currentlyDisconnected ? WorkerState.Open         : currentState;
                        else
                            updated[keyValue.Key] = failMultipletime      ? WorkerState.Disconnected : currentState;
                        await _cache.SetWorkerState(keyValue.Key,updated[keyValue.Key]);
                    }
                    _log.Information(JsonConvert.SerializeObject(updated));
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