using SharedHost;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using SystemHub.Interfaces;
using System.Threading.Tasks;
using SharedHost.Auth;
using SharedHost.Models.Hub;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using SharedHost.Models.Cluster;
using System.IO;
using System.Text;
using SharedHost.Models.Device;
using RestSharp;

namespace SystemHub.Services
{
    public class ClusterSocketPool : IClusterSocketPool
    {
        private readonly ConcurrentDictionary<ClusterCredential, WebSocket> _ClusterSocketsPool;

        private readonly SystemConfig _config;

        private readonly RestClient _conductor;
                
        
        public ClusterSocketPool(SystemConfig config)
        {
            _config = config;
            _conductor = new RestClient(config.Conductor+"");
            _ClusterSocketsPool = new ConcurrentDictionary<ClusterCredential, WebSocket>();

            Task.Run(() => ConnectionHeartBeat());
            Task.Run(() => ConnectionStateCheck());
        }



        public async Task ConnectionHeartBeat()
        {
            try
            {        
                foreach (var socket in _ClusterSocketsPool)
                {
                    if (socket.Value.State == WebSocketState.Open)
                    {
                        SendMessage(socket.Value, "ping");
                    }
                }
                Thread.Sleep(30*1000);
            }catch
            {
                await ConnectionHeartBeat();
            }
        }

        public async Task ConnectionStateCheck()
        {
            try
            {                
                foreach (var socket in _ClusterSocketsPool)
                {
                    if (socket.Value.State == WebSocketState.Closed)
                    {
                        _ClusterSocketsPool.TryRemove(socket);
                    }
                }
                Thread.Sleep(100);
            }catch
            {
                await ConnectionStateCheck();
            }
        }


        public async void AddtoPool(ClusterCredential resp,WebSocket session)
        {
            foreach(var socket in _ClusterSocketsPool)
            {
                if(socket.Key.ID == resp.ID)
                {
                    return;
                }
            }
            _ClusterSocketsPool.TryAdd(resp, session);
            await Handle(resp, session);
        }




        public async Task SendToNode(Message message)
        {
            int NodeID = (int)message.WorkerID;
            foreach (var cluster in _ClusterSocketsPool)
            {
                foreach (var device in cluster.Key.Devices)
                {
                    if(device.ID == NodeID)
                    {
                        SendMessage(cluster.Value,JsonConvert.SerializeObject(message));
                    }
                }                
            }
        }

        public async Task SendToCluster(int ClusterID, Message message)
        {
            int NodeID = (int)message.WorkerID;
            foreach (var cluster in _ClusterSocketsPool)
            {
                if(cluster.Key.ID == ClusterID)
                {
                    SendMessage(cluster.Value,JsonConvert.SerializeObject(message));
                }
            }
        }
















        public async Task Handle(ClusterCredential cred, WebSocket ws)
        {
            try
            {
                WebSocketReceiveResult message;
                do
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        message = await ReceiveMessage(ws, memoryStream);
                        if (message.Count > 0)
                        {
                            var receivedMessage = Encoding.UTF8.GetString(memoryStream.ToArray());
                            var WsMessage = JsonConvert.DeserializeObject<Message>(receivedMessage);
                            switch (WsMessage.Opcode)
                            {
                                case Opcode.STATE_SYNCING:
                                    await HandleWorkerSync(WsMessage);
                                    break;
                                case Opcode.REGISTER_WORKER_NODE:
                                    await HandleWorkerRegister(WsMessage,cred);
                                    break;
                            }
                        }
                    }
                } while (message.MessageType != WebSocketMessageType.Close && ws.State == WebSocketState.Open);
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
            catch
            {
                return;
            }
        }

        async Task HandleWorkerSync(Message message)
        {
            var syncrequest = new RestRequest("State")
                .AddQueryParameter("NewState", message.Data)
                .AddQueryParameter("ID",message.WorkerID.ToString());
            syncrequest.Method = Method.POST;

            await _conductor.ExecuteAsync(syncrequest);
        }
        async Task HandleWorkerRegister(Message message)
        {
            var syncrequest = new RestRequest("State")
                .AddQueryParameter("NewState", message.Data)
                .AddQueryParameter("ID",message.WorkerID.ToString());
            syncrequest.Method = Method.POST;

            await _conductor.ExecuteAsync(syncrequest);
        }

        async Task HandleWorkerRegister(Message message,ClusterCredential cred)
        {
            var syncrequest = new RestRequest("Register")
                .AddQueryParameter("ClusterID",cred.ID.ToString())
                .AddJsonBody(JsonConvert.DeserializeObject<WorkerRegisterModel>(message.Data));
            syncrequest.Method = Method.POST;

            await _conductor.ExecuteAsync(syncrequest);
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

        public void SendMessage(WebSocket ws, string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            var buffer = new ArraySegment<byte>(bytes);
            ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
