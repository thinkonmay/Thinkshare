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
using Microsoft.Extensions.Options;
using DbSchema.CachedState;

namespace SystemHub.Services
{
    public class ClusterSocketPool : IClusterSocketPool
    {
        private readonly ConcurrentDictionary<ClusterCredential, WebSocket> _ClusterSocketsPool;

        private readonly SystemConfig _config;

        private readonly RestClient _conductor;

        private readonly IGlobalStateStore _cache;
                
        
        public ClusterSocketPool(IOptions<SystemConfig> config, IGlobalStateStore cache)
        {
            _config = config.Value;
            _cache = cache;
            _conductor = new RestClient(_config.Conductor+"/Sync");
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
                        await SendMessage(socket.Value, "ping");
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


        public async Task AddtoPool(ClusterCredential resp,WebSocket session)
        {
            bool result = _ClusterSocketsPool.TryAdd(resp, session);
            if(!result)
            {
                return;
            }
            await Handle(resp, session);
            _ClusterSocketsPool.Remove(resp, out session);
        }




        public async Task SendToNode(Message message)
        {
            int NodeID = (int)message.WorkerID;
            foreach (var cluster in _ClusterSocketsPool)
            {
                var request = new RestRequest("GetNode")
                    .AddQueryParameter("ClusterID",cluster.Key.ID.ToString());
                var result = _conductor.Execute(request);

                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    continue;
                }
                var devices = JsonConvert.DeserializeObject<List<WorkerNode>>(result.Content);
                foreach (var device in devices)
                {
                    if(device.ID == NodeID)
                    {
                        await SendMessage(cluster.Value,JsonConvert.SerializeObject(message));
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
                    await SendMessage(cluster.Value,JsonConvert.SerializeObject(message));
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
                                    await onClusterSnapshoot(WsMessage,cred);
                                    break;
                            }
                        }
                    }
                } while (message.MessageType != WebSocketMessageType.Close && ws.State == WebSocketState.Open);
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Serilog.Log.Information("Cluster connection closed due to "+ex.Message);
                return;
            }
        }

        async Task onClusterSnapshoot(Message message, ClusterCredential cred)
        {
            Dictionary<int, string> clusterSnapshoot = JsonConvert.DeserializeObject<Dictionary<int, string>>(message.Data);
            Dictionary<int, string> unsyncedSnapshoot = await _cache.GetClusterSnapshot(cred.ID);
            var syncedSnapshoot = new Dictionary<int, string>();

            foreach (var unsyncedItem in unsyncedSnapshoot)
            {
                bool success = clusterSnapshoot.TryGetValue(unsyncedItem.Key,out var syncedState );


                // if unsynced item is exist in new snapshoot
                if(success)
                {
                    if(syncedState != unsyncedItem.Value)
                    {
                        await _cache.SetClusterSnapshot(cred.ID,syncedSnapshoot);

                        var syncrequest = new RestRequest("State")
                            .AddQueryParameter("NewState", syncedState)
                            .AddQueryParameter("ID",unsyncedItem.Key.ToString());
                        syncrequest.Method = Method.POST;

                        await _conductor.ExecuteAsync(syncrequest);
                    }
                    syncedSnapshoot.Add(unsyncedItem.Key,syncedState);
                }
                else // otherwise, set item as disconnected state
                {
                    await _cache.SetClusterSnapshot(cred.ID,syncedSnapshoot);

                    var syncrequest = new RestRequest("State")
                        .AddQueryParameter("NewState", WorkerState.Disconnected)
                        .AddQueryParameter("ID",unsyncedItem.Key.ToString());
                    syncrequest.Method = Method.POST;

                    await _conductor.ExecuteAsync(syncrequest);
                    
                }
            }

            // find any item that has not been synced yet => unregistered 
            foreach (var item in clusterSnapshoot)
            {
                bool success = syncedSnapshoot.ContainsKey(item.Key);

                // if item is exist in cluster snapshoot but not in synced, add it
                if (!success)
                {
                    syncedSnapshoot.Add(item.Key,item.Value);
                }
            }

            var reply = new Message
            {
                From = Module.HOST_MODULE,
                To = Module.CLUSTER_MODULE,
                Opcode = Opcode.STATE_SYNCING,
                Data = JsonConvert.SerializeObject(syncedSnapshoot)
            };
            await _cache.SetClusterSnapshot(cred.ID,syncedSnapshoot);
            await SendToCluster(cred.ID,reply);
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

        public async Task SendMessage(WebSocket ws, string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            var buffer = new ArraySegment<byte>(bytes);
            await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
