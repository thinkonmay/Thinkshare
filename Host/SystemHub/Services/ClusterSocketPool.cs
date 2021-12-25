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
                
        
        public ClusterSocketPool(IOptions<SystemConfig> config, 
                                 IGlobalStateStore cache)
        {
            _config = config.Value;
            _cache = cache;
            _conductor = new RestClient(_config.Conductor+"/Sync");
            _ClusterSocketsPool = new ConcurrentDictionary<ClusterCredential, WebSocket>();

            Task.Run(() => ConnectionHeartBeat());
        }



        public async Task ConnectionHeartBeat()
        {
            try
            {        
                while (true)
                {
                    foreach (var socket in _ClusterSocketsPool)
                    {
                        SendMessage(socket.Value, "ping");
                    }
                    Thread.Sleep((int)TimeSpan.FromSeconds(4).TotalMilliseconds);
                }
            }catch (Exception ex)
            {
                Serilog.Log.Information("Fail to ping client: " + ex.Message + "\n" + ex.StackTrace );
                await ConnectionHeartBeat();
            }
        }



        public async Task AddtoPool(ClusterCredential resp,WebSocket session)
        {
            bool result = _ClusterSocketsPool.TryAdd(resp, session);
            var snapshoot = await _cache.GetClusterSnapshot(resp.ID);
            if(!result)
            {
                return;
            }

            await Handle(resp, session);

            /// report to conductor
            var request = new RestRequest("Cluster/Disconnected")
                .AddQueryParameter("ClusterID",resp.ID.ToString());
            request.Method = Method.POST;

            await _conductor.ExecuteAsync(request);

            // set all devices state to disconnected
            snapshoot = await _cache.GetClusterSnapshot(resp.ID);
            var newsnapshoot = new Dictionary<int, string>();
            foreach (var Item in snapshoot)
            {
                Serilog.Log.Information("cluster disconnected, worker state set to disconncted");
                newsnapshoot.Add(Item.Key,WorkerState.Disconnected);
                Serilog.Log.Information("WorkerID: "+Item.Key.ToString()+" | State: "+WorkerState.Disconnected);
            }
            await _cache.SetClusterSnapshot(resp.ID,newsnapshoot);
            var done = _ClusterSocketsPool.TryRemove(resp,out var output);
        }




        public async Task SendToNode(Message message)
        {
            int NodeID = (int)message.WorkerID;
            foreach (var cluster in _ClusterSocketsPool)
            {
                var snapshoot = await _cache.GetClusterSnapshot(cluster.Key.ID);
                if(snapshoot.ContainsKey(NodeID))
                {
                    await SendMessage(cluster.Value,JsonConvert.SerializeObject(message));
                }
            }
        }

        public async Task SendToCluster(int ClusterID, Message message)
        {
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
                            try
                            {
                                switch (WsMessage.Opcode)
                                {
                                    case Opcode.STATE_SYNCING:
                                        onClusterSnapshoot(WsMessage,cred);
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Serilog.Log.Information("Fail to parse cluster message");
                                Serilog.Log.Information(ex.Message);
                                Serilog.Log.Information(ex.StackTrace);
                            }
                        }
                    }
                } while (message.MessageType != WebSocketMessageType.Close && ws.State == WebSocketState.Open);
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Serilog.Log.Information("Cluster connection closed");
                Serilog.Log.Information(ex.Message);
                Serilog.Log.Information(ex.StackTrace);
                return;
            }
        }

        async Task onClusterSnapshoot(Message message, ClusterCredential cred)
        {
            Serilog.Log.Information("Got sync state message from cluster, syncing with globalhub");
            Serilog.Log.Information("Before syncing:");
            Dictionary<int, string> clusterSnapshoot = JsonConvert.DeserializeObject<Dictionary<int, string>>(message.Data);
            Dictionary<int, string>? unsyncedSnapshoot = await _cache.GetClusterSnapshot(cred.ID);

            foreach (var item in unsyncedSnapshoot)
            {
                Serilog.Log.Information("WorkerID: "+item.Key.ToString()+" | State: "+item.Value);
            }


            var syncedSnapshoot = new Dictionary<int, string>();

            foreach (var unsyncedItem in unsyncedSnapshoot)
            {
                bool success = clusterSnapshoot.TryGetValue(unsyncedItem.Key,out var syncedState );


                // if unsynced item is exist in new snapshoot
                if(success)
                {
                    if(syncedState != unsyncedItem.Value)
                    {
                        var syncrequest = new RestRequest("Worker/State")
                            .AddQueryParameter("NewState", syncedState)
                            .AddQueryParameter("ID",unsyncedItem.Key.ToString());

                        Serilog.Log.Information("Reporting sync event to conductor: ");
                        Serilog.Log.Information("WorkerID: "+unsyncedItem.Key.ToString()+" | State: "+syncedState);
                        
                        syncrequest.Method = Method.POST;

                        var result = await _conductor.ExecuteAsync(syncrequest);
                        if(result.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            Serilog.Log.Information("Reportd state changing to conductor");
                        }
                        else
                        {
                            Serilog.Log.Information("Fail to report event to conductor");
                        }
                    }

                    syncedSnapshoot.Add(unsyncedItem.Key,syncedState);
                }
                else // otherwise, set item as missing state
                {
                    var syncrequest = new RestRequest("Worker/State")
                        .AddQueryParameter("NewState", WorkerState.MISSING)
                        .AddQueryParameter("ID",unsyncedItem.Key.ToString());
                    syncrequest.Method = Method.POST;
                    
                    Serilog.Log.Information("Reporting sync event to conductor: ");
                    Serilog.Log.Information("WorkerID: "+unsyncedItem.Key.ToString()+" | State: Missing");

                    var result = await _conductor.ExecuteAsync(syncrequest);
                    if(result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Serilog.Log.Information("Reportd state changing to conductor");
                    }
                    else
                    {
                        Serilog.Log.Information("Fail to report event to conductor");
                    }
                    syncedSnapshoot.Add(unsyncedItem.Key,WorkerState.MISSING);
                }
            }

            // find any item that has not been synced yet => add to snapshoot 
            foreach (var item in clusterSnapshoot)
            {
                bool success = syncedSnapshoot.ContainsKey(item.Key);

                // if item is exist in cluster snapshoot but not in synced, add it
                if (!success)
                {
                    Serilog.Log.Information("New worker node sync to host");
                    var workerInfor = _cache.GetWorkerInfor(item.Key);
                    if(workerInfor == null)
                    {
                        Serilog.Log.Information("Cannot find worker infor in cache");
                        syncedSnapshoot.Add(item.Key,WorkerState.unregister);
                    }
                    else
                    {
                        Serilog.Log.Information("Found worker infor in cache, added to synced cluster snapshoot");
                        syncedSnapshoot.Add(item.Key,item.Value);
                    }
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


            Serilog.Log.Information("State syncing done, after syncing: ");
            foreach (var item in syncedSnapshoot)
            {
                Serilog.Log.Information("WorkerID: "+item.Key.ToString()+" | State: "+item.Value);
            }
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
            try
            {
                await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            } catch { Serilog.Log.Information("Fail to send websocket to client"); }
        }
    }
}
