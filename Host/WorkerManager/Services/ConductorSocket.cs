using WorkerManager.Interfaces;
using Microsoft.Extensions.Options;
using SharedHost.Models.Cluster;
using System.Threading.Tasks;
using SharedHost.Models.Device;
using System.Collections.Generic;
using SharedHost.Models.Shell;
using RestSharp;
using System.Net;
using Newtonsoft.Json;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System;
using System.IO;
using System.Text;
using SharedHost.Models.Session;
using DbSchema.SystemDb;
using SharedHost.Models.Local;
using DbSchema.CachedState;
using SharedHost;

namespace WorkerManager.Services
{


    public class ConductorSocket : IConductorSocket
    {
        private readonly ClusterConfig clusterConfig;

        private readonly ClusterDbContext _db;

        private ClientWebSocket _clientWebSocket;

        private RestClient _scriptmodel;

        private ILocalStateStore _cache;

        private readonly ClusterConfig _config;

        public ConductorSocket(IOptions<ClusterConfig> cluster, 
                               ClusterDbContext db,
                               ILocalStateStore cache)
        {
            _db = db;
            _cache = cache;
            _config = cluster.Value;
            _scriptmodel = new RestClient();
        }

        public async Task<bool> Start()
        {
            try
            {
                var token = (string)_db.Clusters.First().Token;
                _clientWebSocket = new ClientWebSocket();
                await _clientWebSocket.ConnectAsync(
                    new Uri(clusterConfig.ClusterHub+"?token=" + token), 
                    CancellationToken.None);

                if (_clientWebSocket.State == WebSocketState.Open)
                {
                    Task.Run(() => Handle());
                    var currentState = _cache.GetClusterState();
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
            catch (Exception ex)
            { 
                Serilog.Log.Information("fail to connect to system hub due to " + ex.Message);
                Thread.Sleep(((int)TimeSpan.FromSeconds(10).TotalMilliseconds));
                _clientWebSocket = null;
                return false;
            }
        }

        public async Task<bool> Stop()
        {
            if(_clientWebSocket.State == WebSocketState.Open)
            {
                await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
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
                    using (var memoryStream = new MemoryStream())
                    {
                        message = await ReceiveMessage(_clientWebSocket, memoryStream);
                        if (message.Count > 0)
                        {
                            var receivedMessage = Encoding.UTF8.GetString(memoryStream.ToArray());
                            if (receivedMessage == "ping") { continue; }
                            var WsMessage = JsonConvert.DeserializeObject<Message>(receivedMessage);
                            switch (WsMessage.Opcode)
                            {
                                // execute session operation by public id 
                                case Opcode.SESSION_INITIALIZE:
                                    await Initialize((int)WsMessage.WorkerID, WsMessage.token);
                                    break;
                                case Opcode.SESSION_TERMINATE:
                                    await Terminate((int)WsMessage.WorkerID);
                                    break;
                                case Opcode.SESSION_RECONNECT:
                                    await Reconnect((int)WsMessage.WorkerID);
                                    break;
                                case Opcode.SESSION_DISCONNECT:
                                    await Disconnect((int)WsMessage.WorkerID);
                                    break;
                                case Opcode.STATE_SYNCING:
                                    await StateSyncing(WsMessage); 
                                    break;
                                case Opcode.WORKER_INFOR_REQUEST:
                                    await GetWorkerInfor(WsMessage);
                                    break;
                            }
                        }
                    }
                } while (message.MessageType != WebSocketMessageType.Close && _clientWebSocket.State == WebSocketState.Open);
                await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Serilog.Log.Information("fail to connect to system hub due to " + ex.Message);
                Serilog.Log.Information("Retry after 10s");
                Thread.Sleep(((int)TimeSpan.FromSeconds(10).TotalMilliseconds));
                await Start();
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
            await _clientWebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }









        async Task StateSyncing(Message message)
        {
            Dictionary<int, string> syncedState = JsonConvert.DeserializeObject<Dictionary<int,string>>(message.Data); 

            try
            {
                while (true)
                {
                    var clusterSnapshoot = await _cache.GetClusterState();
                    /// check for every different between 
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
                        }
                        else // compare state and sync for any different
                        {
                            if(output != item.Value)
                            {
                                var request = new Message
                                {
                                    Opcode = Opcode.STATE_SYNCING,
                                    From = Module.CLUSTER_MODULE,
                                    To = Module.HOST_MODULE,
                                    Data = JsonConvert.SerializeObject(clusterSnapshoot)
                                };
                                await SendMessage(JsonConvert.SerializeObject(request));
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
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Information("state syncing failed due to " + ex.Message);
                await StateSyncing(message);
            }
        }




        async Task GetWorkerInfor(Message message)
        {
            var worker = _db.Devices.Find(message.WorkerID);
            var registration = new WorkerRegisterModel
            {
                PrivateID = worker.ID,
                CPU = worker.CPU,
                GPU = worker.GPU,
                RAMcapacity = worker.RAMcapacity,
                OS = worker.OS,
            };

            var reply = new Message
            {
                From = Module.CLUSTER_MODULE,
                To = Module.HOST_MODULE,
                Opcode = Opcode.REGISTER_WORKER_NODE,
                Data = JsonConvert.SerializeObject(registration)
            };
            await SendMessage(JsonConvert.SerializeObject(reply));
        }





















        /// <summary>
        /// initialize session
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public async Task Initialize(int ID, string token)
        {
            var worker = _db.Devices.Where(o => o.ID == ID).First();
            worker.RestoreWorkerNode();

            worker.RemoteToken = token;
            if (await worker.SessionInitialize())
            {
                await _cache.SetWorkerState(worker.ID, WorkerState.OnSession);
            }
            else 
            {
                await _cache.SetWorkerState(worker.ID, WorkerState.OffRemote);
            }
        }


        /// <summary>
        /// Terminate session 
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        public async Task Terminate(int GlobalID)
        {
            var worker = _db.Devices.Where(o => o.ID == GlobalID).First();
            worker.RestoreWorkerNode();

            worker.RemoteToken = null;
            if(await worker.SessionTerminate())
            {
                await _cache.SetWorkerState(worker.ID, WorkerState.Open);
            }
            await _db.SaveChangesAsync();
        }


        /// <summary>
        /// disconnect remote control during session
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        public async Task Disconnect(int GlobalID)
        {
            var worker = _db.Devices.Where(o => o.ID == GlobalID).First();
            worker.RestoreWorkerNode();

            if (await worker.SessionDisconnect())
            {
                await _cache.SetWorkerState(worker.ID,WorkerState.OffRemote);
            }
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Reconnect remote control after disconnect
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        public async Task Reconnect(int GlobalID)
        {
            var worker = _db.Devices.Where(o => o.ID == GlobalID).First();
            worker.RestoreWorkerNode();

            if (await worker.SessionReconnect())
            {
                await _cache.SetWorkerState(worker.ID, WorkerState.OnSession);
            }
        }

    }
}
