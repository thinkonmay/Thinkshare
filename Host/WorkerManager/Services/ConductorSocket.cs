using WorkerManager.Interfaces;
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
using WorkerManager.Data;
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

        public bool Initialized { get; set; }

        public ConductorSocket(ClusterConfig cluster, 
                               ClusterDbContext db,
                               ILocalStateStore cache)
        {
            _db = db;
            _cache = cache;
            Initialized = false;
            clusterConfig = cluster;
        }

        public async Task<bool> Start()
        {
            Initialized = true;
            try
            {
                var token = (string)_db.Clusters.First().Token;
                _scriptmodel = new RestClient("https://" + clusterConfig.HostDomain + "/Shell");
                _clientWebSocket = new ClientWebSocket();
                await _clientWebSocket.ConnectAsync(new Uri("wss://" + clusterConfig.HostDomain + "/Hub/Cluster?token=" + token), CancellationToken.None);
                if (_clientWebSocket.State == WebSocketState.Open)
                {
                    Handle();
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
                                case Opcode.ID_GRANT:
                                    await Assignment(WsMessage);
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
                Start();
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


        async Task Assignment(Message message)
        {
            var assign = JsonConvert.DeserializeObject<IDAssign>(message.Data);
            
            var currentState = await _cache.GetWorkerState(assign.PrivateID);
            if(currentState == WorkerState.Registering)
            {
                var worker = _db.Devices.Find(assign.PrivateID);

                worker.GlobalID = assign.GlobalID;
                _db.Update(worker);
                await _db.SaveChangesAsync();
                await _cache.SetWorkerState(assign.PrivateID, WorkerState.Open);
            }            
        }


        /// <summary>
        /// initialize session
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public async Task Initialize(int GlobalID, string token)
        {
            var worker = _db.Devices.Where(o => o.GlobalID == GlobalID).First();
            worker.RestoreWorkerNode();

            worker.RemoteToken = token;
            if (await worker.SessionInitialize())
            {
                await _cache.SetWorkerState(worker.PrivateID, WorkerState.OnSession);
            }
            else 
            {
                await _cache.SetWorkerState(worker.PrivateID, WorkerState.OffRemote);
            }
        }


        /// <summary>
        /// Terminate session 
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        public async Task Terminate(int GlobalID)
        {
            var worker = _db.Devices.Where(o => o.GlobalID == GlobalID).First();
            worker.RestoreWorkerNode();

            worker.RemoteToken = null;
            if(await worker.SessionTerminate())
            {
                await _cache.SetWorkerState(worker.PrivateID, WorkerState.Open);
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
            var worker = _db.Devices.Where(o => o.GlobalID == GlobalID).First();
            worker.RestoreWorkerNode();

            if (await worker.SessionDisconnect())
            {
                await _cache.SetWorkerState(worker.PrivateID,WorkerState.OffRemote);
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
            var worker = _db.Devices.Where(o => o.GlobalID == GlobalID).First();
            worker.RestoreWorkerNode();

            if (await worker.SessionReconnect())
            {
                await _cache.SetWorkerState(worker.PrivateID, WorkerState.OnSession);
            }
        }





        /// <summary>
        /// Report session state change to user 
        /// </summary>
        public async Task WorkerStateSyncing(int WorkerID, string WorkerState)
        {
            await SendMessage(JsonConvert.SerializeObject( 
                new Message { WorkerID = WorkerID, 
                            From = Module.CLUSTER_MODULE, 
                            To= Module.HOST_MODULE, 
                            Opcode=Opcode.STATE_SYNCING,
                            Data= WorkerState}));
            return;
        }



        public async Task ReportWorkerRegistered(ClusterWorkerNode information)
        {
            var WorkerNode = new WorkerNode{
                OS = information.OS,
                GPU = information.GPU,
                CPU = information.CPU,
                RAMcapacity = information.RAMcapacity
            };
            await SendMessage(JsonConvert.SerializeObject(
                new Message { WorkerID = information.PrivateID, 
                            From = Module.CLUSTER_MODULE, 
                            To= Module.HOST_MODULE, 
                            Opcode=Opcode.REGISTER_WORKER_NODE,
                            Data= JsonConvert.SerializeObject(WorkerNode)}));
        }


        public async Task<List<ScriptModel>> GetDefaultModel()
        {
            if(!_db.ScriptModels.Any())
            {
                var token = _db.Owner.First().token;
                var request = new RestRequest("AllModel")
                    .AddHeader("Authorization", "Bearer " + token);
                request.Method = Method.GET;

                var result = await _scriptmodel.ExecuteAsync(request);
                if(result.StatusCode == HttpStatusCode.OK)
                {
                    var allModel = JsonConvert.DeserializeObject<ICollection<ScriptModel>>(result.Content);
                    var defaultModel = allModel.Where(o => o.ID < (int)ScriptModelEnum.LAST_DEFAULT_MODEL).ToList();
                    _db.ScriptModels.AddRange(defaultModel);
                    return defaultModel;
                }
                else
                {
                    Serilog.Log.Information("Failed to get default script");
                    return await GetDefaultModel();
                }
            }
            else
            {
                var allModel = _db.ScriptModels.ToList();
                return allModel;
            }
        }
    }
}
