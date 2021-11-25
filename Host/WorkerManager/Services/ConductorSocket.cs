using WorkerManager.Interfaces;
using SharedHost.Models.Cluster;
using System.Threading.Tasks;
using WorkerManager.Models;
using SharedHost.Models.Device;
using System.Collections.Generic;
using SharedHost.Models.Shell;
using RestSharp;
using System.Net;
using SharedHost;
using Newtonsoft.Json;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System;
using System.IO;
using System.Text;
using SharedHost.Models.Session;
using WorkerManager.Data;

namespace WorkerManager.Services
{


    public class ConductorSocket : IConductorSocket
    {
        private readonly ClusterConfig clusterConfig;

        private readonly ClusterDbContext _db;

        private ClientWebSocket _clientWebSocket;

        private RestClient _scriptmodel;

        public ConductorSocket(ClusterConfig cluster, 
                               ClusterDbContext db)
        {
            _db = db;
            clusterConfig = cluster;
            Task.Run(() => TokenLookup());
        }


        public async Task TokenLookup()
        {
            try
            {
                var token = (string)_db.Clusters.First().Token;
                _scriptmodel = new RestClient("https://"+clusterConfig.HostDomain+"/Shell");
                _clientWebSocket = new ClientWebSocket();
                await _clientWebSocket.ConnectAsync(new Uri("wss://"+clusterConfig.HostDomain+"/Hub/Cluster?token="+token), CancellationToken.None);
                if(_clientWebSocket.State == WebSocketState.Open)
                {
                    await Handle();
                    return;
                }
                return;
            }
            catch (Exception ex)
            {
                Serilog.Log.Information("fail to connect to system hub due to "+ex.Message);
                Thread.Sleep(((int)TimeSpan.FromSeconds(10).TotalMilliseconds));
                _clientWebSocket = null;
                await TokenLookup();
            }
        }



        public async Task Handle()
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
                        if(receivedMessage == "ping"){continue;}
                        var WsMessage = JsonConvert.DeserializeObject<Message>(receivedMessage);
                        switch(WsMessage.Opcode)
                        {
                            // execute session operation by public id 
                            case Opcode.SESSION_INITIALIZE:
                                await Initialize((int)WsMessage.WorkerID,WsMessage.token,JsonConvert.DeserializeObject<SessionBase>(WsMessage.Data));
                                break;
                            case Opcode.SESSION_TERMINATE:
                                await Terminate((int)WsMessage.WorkerID);
                                break;
                            case Opcode.SESSION_RECONNECT:
                                await Reconnect((int)WsMessage.WorkerID,JsonConvert.DeserializeObject<SessionBase>(WsMessage.Data));
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
            var worker = _db.Devices.Find(assign.PrivateID);

            worker.GlobalID = assign.GlobalID;
            worker._workerState = WorkerState.Open;
            await _db.SaveChangesAsync();
        }


        /// <summary>
        /// initialize session
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public async Task Initialize(int GlobalID, string token, SessionBase session)
        {
            var worker = _db.Devices.Where(o => o.GlobalID == GlobalID).First();
            worker.RestoreWorkerNode();

            worker.RemoteToken = token;
            worker.QoE = session.QoE;
            worker.SignallingUrl = session.SignallingUrl;
            await worker.SessionInitialize();
            await _db.SaveChangesAsync();
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
            worker.SignallingUrl = null;
            worker.QoE = null;
            await worker.SessionTerminate();
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

            await worker.SessionDisconnect();
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Reconnect remote control after disconnect
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        public async Task Reconnect(int GlobalID, SessionBase session)
        {
            var worker = _db.Devices.Where(o => o.GlobalID == GlobalID).First();
            worker.RestoreWorkerNode();

            worker.QoE = session.QoE;
            worker.SignallingUrl = session.SignallingUrl;
            await worker.SessionReconnect();
            await _db.SaveChangesAsync();
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
                RAMcapacity = information.RAMcapacity,
                WorkerState = information._workerState
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
