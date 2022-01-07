using WorkerManager.Interfaces;
using Microsoft.Extensions.Options;
using SharedHost.Models.Cluster;
using System.Threading.Tasks;
using SharedHost.Models.Device;
using System.Collections.Generic;
using RestSharp;
using System.Net;
using Newtonsoft.Json;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System;
using System.IO;
using System.Text;
using WorkerManager;
using SharedHost;

using WorkerManager.Models;

namespace WorkerManager.Services
{


    public class ConductorSocket : IConductorSocket
    {
        private readonly ClusterConfig clusterConfig;

        private ClientWebSocket _clientWebSocket;

        private RestClient _scriptmodel;

        private ILocalStateStore _cache;

        private readonly ClusterConfig _config;
        
        private readonly ITokenGenerator _generator;

        private readonly ClusterKey _cluster;

        private bool isRunning;

        public ConductorSocket(IOptions<ClusterConfig> cluster, 
                               ITokenGenerator generator,
                               ILocalStateStore cache)
        {
            isRunning = false;
            _generator = generator;
            _cache = cache;
            _config = cluster.Value;
            _scriptmodel = new RestClient();
            _cluster = _cache.GetClusterInfor().Result;
        }

        public async Task<bool> Start()
        {
            try
            {
                if (isRunning) { return false; }
                var token = (await _cache.GetClusterInfor()).ClusterToken;
                _clientWebSocket = new ClientWebSocket();
                await _clientWebSocket.ConnectAsync(
                    new Uri(_config.ClusterHub+"?token=" + token), 
                    CancellationToken.None);

                if (_clientWebSocket.State == WebSocketState.Open)
                {
                    // DONOT set this to awaited
                    isRunning = true;
                    Handle();
                    var currentState = await _cache.GetClusterState();
                    var request = new Message
                    {
                        Opcode = Opcode.STATE_SYNCING,
                        From = Module.CLUSTER_MODULE,
                        To = Module.HOST_MODULE,
                        Data = JsonConvert.SerializeObject(currentState)
                    };
                    await SendMessage(JsonConvert.SerializeObject(request));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch 
            { 
                return false;
            }
        }

        public async Task<bool> Stop()
        {
            if(isRunning)
            {
                isRunning = false;
                return true;
            }
            else
            {
                return false;
            }
        }





        public async Task Handle()
        {
            try
            {
                WebSocketReceiveResult message;
                do
                {
                    if (!isRunning) 
                    { 
                        await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        return; 
                    }
                    using (var memoryStream = new MemoryStream())
                    {
                        message = await ReceiveMessage(_clientWebSocket, memoryStream);
                        if (message.Count > 0)
                        {
                            var receivedMessage = Encoding.UTF8.GetString(memoryStream.ToArray());
                            if (receivedMessage == "ping") { Serilog.Log.Information("Got ping from system hub"); continue; }
                            var WsMessage = JsonConvert.DeserializeObject<Message>(receivedMessage);
                            switch (WsMessage.Opcode)
                            {
                                // execute session operation by public id 
                                // DONOT await this function because it will stop other message
                                case Opcode.SESSION_INITIALIZE:
                                    Initialize((int)WsMessage.WorkerID, WsMessage.token);
                                    break;
                                case Opcode.SESSION_TERMINATE:
                                    Terminate((int)WsMessage.WorkerID);
                                    break;
                                case Opcode.SESSION_RECONNECT:
                                    Reconnect((int)WsMessage.WorkerID);
                                    break;
                                case Opcode.SESSION_DISCONNECT:
                                    Disconnect((int)WsMessage.WorkerID);
                                    break;
                                case Opcode.STATE_SYNCING:
                                    StateSyncing(WsMessage); 
                                    break;
                            }
                        }
                    }
                } while (message.MessageType != WebSocketMessageType.Close && _clientWebSocket.State == WebSocketState.Open);
                await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                RestoreConnection(null);
            }
            catch (Exception ex)
            {
                RestoreConnection(ex);
            }
        }

        async Task RestoreConnection(Exception? exception)
        {
            try
            {
                await Stop();
                if(exception != null)
                {
                    Serilog.Log.Information("Error when connect to sytemhub: " + exception.Message);
                    Serilog.Log.Information(exception.StackTrace);
                    Serilog.Log.Information("Retry after 10 second");
                }
                else
                {
                    Serilog.Log.Information("Disconnected from systemhub");
                    Serilog.Log.Information("Retry after 10 second");
                }
                Thread.Sleep(((int)TimeSpan.FromSeconds(10).TotalMilliseconds));
                var success = await Start();
                if(!success)
                {
                    await RestoreConnection(null);
                }

            }
            catch (Exception ex)
            {
                await RestoreConnection(ex);
            }
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

        public async Task SendMessage(string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            var buffer = new ArraySegment<byte>(bytes);

            try
            {
                await _clientWebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            } catch (Exception ex)
            { 
                Serilog.Log.Information("Fail to send message to websocket client"); 
                Thread.Sleep(1000);
                await SendMessage(msg);
            }
        }









        async Task StateSyncing(Message message)
        {
            Dictionary<int, string> syncedState = JsonConvert.DeserializeObject<Dictionary<int,string>>(message.Data); 

            try
            {
                while (true)
                {
                    var clusterSnapshoot = await _cache.GetClusterState();
                    // check for every different between 
                    foreach ( var item in clusterSnapshoot)
                    {
                        bool success = syncedState.TryGetValue(item.Key,out var output);

                        // if item is not exist in synced snapshoot, sync
                        if(!success)
                        {
                            var request = new Message
                            {
                                Opcode = Opcode.STATE_SYNCING,
                                From = Module.CLUSTER_MODULE,
                                To = Module.HOST_MODULE,
                                Data = JsonConvert.SerializeObject(clusterSnapshoot)
                            };
                            await SendMessage(JsonConvert.SerializeObject(request));
                            return;
                        }
                        else // compare state and sync for any different
                        {
                            if(output != item.Value)
                            {
                                if(item.Value == WorkerState.unregister)
                                {
                                    var device = await _cache.GetWorkerInfor(item.Key);
                                    if(device == null)
                                    {
                                        clusterSnapshoot.Remove(item.Key);
                                        await _cache.SetWorkerState(item.Key,null);
                                    }
                                    else
                                    {
                                        bool result = await reRegisterDevice((ClusterWorkerNode)device);
                                        if(!result)
                                        {
                                            clusterSnapshoot.Remove(item.Key);
                                            await _cache.SetWorkerState(item.Key,null);
                                        }
                                    }
                                }

                                var request = new Message
                                {
                                    Opcode = Opcode.STATE_SYNCING,
                                    From = Module.CLUSTER_MODULE,
                                    To = Module.HOST_MODULE,
                                    Data = JsonConvert.SerializeObject(clusterSnapshoot)
                                };
                                await SendMessage(JsonConvert.SerializeObject(request));
                                return;
                            }
                        }
                    }

                    // check for item that is synced with host but not exist in cluster 
                    foreach (var item in syncedState)
                    {
                        bool success = clusterSnapshoot.TryGetValue(item.Key,out var output);
                        if(!success)
                        {
                            await _cache.SetWorkerState(item.Key,WorkerState.MISSING);
                        }
                    }
                    Thread.Sleep((int)TimeSpan.FromMilliseconds(50).TotalMilliseconds);
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Information("state syncing failed");
                Serilog.Log.Information(ex.Message);
                Serilog.Log.Information(ex.StackTrace);
                await StateSyncing(message);
            }
        }



        async Task<bool> reRegisterDevice(ClusterWorkerNode model)
        {

            var workers = await _cache.GetClusterState();
            foreach (var item in workers)
            {
                await _cache.SetWorkerState(item.Key,WorkerState.Disconnected);
            }



            var client = new RestClient();
            var request = new RestRequest(_config.WorkerRegisterUrl)
                .AddQueryParameter("ClusterName",_cluster.Name)
                .AddJsonBody(model.model)
                .AddHeader("Authorization","Bearer "+_cluster.OwnerToken);

            var result = await client.ExecuteAsync(request);
            if(result.StatusCode == HttpStatusCode.OK)
            {
                int id = JsonConvert.DeserializeObject<int>(result.Content);
                model.ID = id;

                await _cache.CacheWorkerInfor(model);
                await _cache.SetWorkerState(model.ID, WorkerState.Open);
                return true;
            }
            else
            {
                Serilog.Log.Information("Fail to register device");
                return false;
            }
        }









        




        public async Task Initialize(int ID, string remoteToken)
        {
            var worker = await _cache.GetWorkerInfor(ID);

            await _cache.SetWorkerRemoteToken(ID,remoteToken);

            var workerToken = await _generator.GenerateWorkerToken(worker);
            if (await worker.SessionInitialize(workerToken))
            {
                Serilog.Log.Information("initialize session done");
                await _cache.SetWorkerState(worker.ID, WorkerState.OnSession);
            }
            else 
            {
                Serilog.Log.Information("Fail to initialize session");
                await _cache.SetWorkerState(worker.ID, WorkerState.OffRemote);
            }
        }



        public async Task Terminate(int GlobalID)
        {
            var worker = await _cache.GetWorkerInfor(GlobalID);
            await _cache.SetWorkerRemoteToken(GlobalID,"none");

            var workerToken = await _generator.GenerateWorkerToken(worker);
            if(await worker.SessionTerminate(workerToken))
            {
                Serilog.Log.Information("Terminate session success");
                await _cache.SetWorkerState(worker.ID, WorkerState.Open);
            }
            else
            {
                Serilog.Log.Information("Fail to terminate session");
            }
        }


        public async Task Disconnect(int GlobalID)
        {
            var worker = await _cache.GetWorkerInfor(GlobalID);

            var workerToken = await _generator.GenerateWorkerToken(worker);
            if (await worker.SessionDisconnect(workerToken))
            {
                await _cache.SetWorkerState(worker.ID,WorkerState.OffRemote);
            }
        }

        public async Task Reconnect(int GlobalID)
        {
            var worker = await _cache.GetWorkerInfor(GlobalID);

            var workerToken = await _generator.GenerateWorkerToken(worker);
            if (await worker.SessionReconnect(workerToken))
            {
                await _cache.SetWorkerState(worker.ID, WorkerState.OnSession);
            }
        }
    }
}
