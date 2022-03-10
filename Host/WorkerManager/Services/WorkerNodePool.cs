using System;
using System.Threading;
using System.Linq;
using WorkerManager.Interfaces;
using SharedHost.Models.Device;
using System.Threading.Tasks;
using WorkerManager.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.IO;


namespace WorkerManager.Services
{
    public class WorkerNodePool : IWorkerNodePool
    {
        private readonly ILocalStateStore _cache;

        private Task _stateStyncing;
        
        private Task _workerShell;

        private bool isRunning;

        private readonly ILog _log;

        private ConcurrentDictionary<int,WebSocket> _pool;

        public WorkerNodePool(ILocalStateStore cache,
                              ILog log)
        {
            _log = log;
            _cache = cache;
            _pool = new ConcurrentDictionary<int,WebSocket>();
            isRunning = false;
        }

        public bool Start()
        {
            if(isRunning)
                return false;

            isRunning = true;
            _workerShell =      Task.Run(() => GetWorkerMetric()); 
            return true;
        }

        public bool Stop()
        {
            if(!isRunning)
                return false;

            isRunning = false;
            _workerShell.Wait();
            return true;
        }

        private async Task<WebSocketReceiveResult> ReceiveMessage(WebSocket ws, Stream memoryStream)
        {
            var readBuffer = new ArraySegment<byte>(new byte[4 * 1024]);
            WebSocketReceiveResult result;
            do
            {
                result = await ws.ReceiveAsync(readBuffer, CancellationToken.None);
                await memoryStream.WriteAsync(readBuffer.Array, readBuffer.Offset, result.Count, CancellationToken.None);
            } while (!result.EndOfMessage);
            return result;
        }



        public async Task HandleWorkerConnection(int ID, WebSocket ws)
        {
            await _cache.SetWorkerState(ID,WorkerState.Open);
            _pool.TryAdd(ID,ws);
            try
            {
                do
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        var message = ReceiveMessage(ws, memoryStream).Result;
                    }
                } while (ws.State == WebSocketState.Open);
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _log.Information("Connection closed");
            }
            _pool.TryRemove(ID,out var rm);
            await _cache.SetWorkerState(ID,WorkerState.Disconnected);
        }

        public async Task SendRequest(int ID, string URL, string Data)
        {
            _pool.TryGetValue(ID,out var ws);
            var msg = JsonConvert.SerializeObject(new
            {
                URL = URL,
                Data = Data,
            });
            var bytes = System.Text.Encoding.UTF8.GetBytes(msg);
            var buffer = new ArraySegment<byte>(bytes);
            await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
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
                        await this.SendRequest(worker.Key,"/Script",model.Script);
                    }
                }
                Thread.Sleep(TimeSpan.FromSeconds(60));
            }
        }
    }
}