using SharedHost;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using SystemHub.Interfaces;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using SharedHost.Models.Cluster;
using System.IO;
using System.Text;
using SharedHost.Models.Device;
using RestSharp;
using Microsoft.Extensions.Options;
using DbSchema.CachedState;
using SharedHost.Logging;

namespace SystemHub.Services
{
    public class ClusterSocketPool : IClusterSocketPool
    {
        private readonly ConcurrentDictionary<ClusterCredential, WebSocket> _ClusterSocketsPool;

        private readonly SystemConfig _config;

        private readonly IGlobalStateStore _cache;
        
        private readonly ILog _log;
                
        
        public ClusterSocketPool(IOptions<SystemConfig> config, 
                                 ILog log,
                                 IGlobalStateStore cache)
        {
            _config = config.Value;
            _cache = cache;
            _log = log;
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
                    Thread.Sleep(TimeSpan.FromSeconds(4));
                }
            }catch (Exception ex)
            {
                _log.Error("Fail to ping client",ex);
                await ConnectionHeartBeat();
            }
        }



        public async Task AddtoPool(ClusterCredential resp,WebSocket session)
        {
            _ClusterSocketsPool.AddOrUpdate(resp, session, (x,y) => session);
            await Handle(resp, session);
            _ClusterSocketsPool.TryRemove(resp,out var removed_ws);

            /// report to conductor
            var request = new RestRequest($"{_config.Conductor}/Sync/Cluster/Disconnected")
                .AddQueryParameter("ClusterID",resp.ID.ToString());
            request.Method = Method.POST;

            await (new RestClient()).ExecuteAsync(request);

            // set all devices state to disconnected
            var snapshoot = await _cache.GetClusterSnapshot(resp.ID);
            var newsnapshoot = new Dictionary<int, string>();
            foreach (var Item in snapshoot)
            {
                _log.Information("cluster disconnected, worker state set to disconncted");
                newsnapshoot.Add(Item.Key,WorkerState.Disconnected);
                _log.Information("WorkerID: "+Item.Key.ToString()+" | State: "+WorkerState.Disconnected);
            }
            await _cache.SetClusterSnapshot(resp.ID,newsnapshoot);
        }





        public async Task SendToCluster(int ClusterID, Message message)
        {
            var ws = _ClusterSocketsPool.Where(x => x.Key.ID == ClusterID).First().Value;
            try { await SendMessage(ws,JsonConvert.SerializeObject(message)); }
            catch (System.Exception ex) { }
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
                                _log.Error("Fail to parse cluster message",ex);
                            }
                        }
                    }
                } while (message.MessageType != WebSocketMessageType.Close && ws.State == WebSocketState.Open);
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _log.Error("Cluster connection closed",ex);
                return;
            }
        }

        async Task onClusterSnapshoot(Message message, ClusterCredential cred)
        {
            _log.Information("Got sync state message from cluster, syncing with globalhub");
            _log.Information("Before syncing:");
            Dictionary<int, string> clusterSnapshoot = JsonConvert.DeserializeObject<Dictionary<int, string>>(message.Data);
            Dictionary<int, string>? unsyncedSnapshoot = await _cache.GetClusterSnapshot(cred.ID);

            foreach (var item in unsyncedSnapshoot)
            {
                _log.Information("WorkerID: "+item.Key.ToString()+" | State: "+item.Value);
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
                        var syncrequest = new RestRequest($"{_config.Conductor}/Sync/Worker/State")
                            .AddQueryParameter("NewState", syncedState)
                            .AddQueryParameter("ID",unsyncedItem.Key.ToString());

                        _log.Information("Reporting sync event to conductor: ");
                        _log.Information("WorkerID: "+unsyncedItem.Key.ToString()+" | State: "+syncedState);
                        
                        syncrequest.Method = Method.POST;

                        var result = await (new RestClient()).ExecuteAsync(syncrequest);
                        if(result.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            _log.Information("Reportd state changing to conductor");
                        }
                        else
                        {
                            _log.Information("Fail to report event to conductor");
                        }
                    }

                    syncedSnapshoot.Add(unsyncedItem.Key,syncedState);
                }
                else 
                {
                    // otherwise, ignore the item
                }
            }

            // find any item that has not been synced yet => add to snapshoot 
            foreach (var item in clusterSnapshoot)
            {
                bool success = syncedSnapshoot.ContainsKey(item.Key);

                // if item is exist in cluster snapshoot but not in synced, add it
                if (!success)
                {
                    _log.Information("New worker node sync to host");
                    var workerInfor = _cache.GetWorkerInfor(item.Key);
                    if(workerInfor == null)
                    {
                        _log.Information("Cannot find worker infor in cache");
                        syncedSnapshoot.Add(item.Key,WorkerState.unregister);
                    }
                    else
                    {
                        _log.Information("Found worker infor in cache, added to synced cluster snapshoot");
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

            try { await _cache.SetClusterSnapshot(cred.ID,syncedSnapshoot); }
            catch (Exception ex) 
            { 
                _log.Error($"Error state syncing",ex); 
            }

            _log.Information("State syncing done, after syncing: ");
            foreach (var item in syncedSnapshoot) { _log.Information("WorkerID: "+item.Key.ToString()+" | State: "+item.Value); }

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
            } catch (Exception ex)
            { 
                _log.Information("Fail to send websocket to client"); 
                Thread.Sleep(1000);
                await SendMessage(ws,msg);
            }
        }
    }
}
