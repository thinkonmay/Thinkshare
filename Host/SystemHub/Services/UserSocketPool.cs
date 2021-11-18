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

namespace SystemHub.Services
{
    public class UserSocketPool : IUserSocketPool
    {
        private readonly ConcurrentDictionary<AuthenticationResponse, WebSocket> _UserSocketsPool;

        private readonly ConcurrentDictionary<AuthenticationResponse, WebSocket> _ClusterSocketsPool;

        private readonly IWebSocketHandler _WebSocketHandler;

        
        
        public UserSocketPool(SystemConfig config,
                             IWebSocketHandler wsHandler)
        {
            _UserSocketsPool =    new ConcurrentDictionary<AuthenticationResponse, WebSocket>();
            _ClusterSocketsPool = new ConcurrentDictionary<AuthenticationResponse, WebSocket>();
            _WebSocketHandler = wsHandler;

            Task.Run(() => ConnectionHeartBeat());
            Task.Run(() => ConnectionStateCheck());
        }



        public async Task ConnectionHeartBeat()
        {
            try
            {                
                foreach (var socket in _UserSocketsPool)
                {
                    if(socket.Value.State == WebSocketState.Open)
                    {
                        _WebSocketHandler.SendMessage(socket.Value,"ping");
                    }
                }
                foreach (var socket in _ClusterSocketsPool)
                {
                    if (socket.Value.State == WebSocketState.Open)
                    {
                        _WebSocketHandler.SendMessage(socket.Value, "ping");
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
                foreach (var socket in _UserSocketsPool)
                {
                    if(socket.Value.State == WebSocketState.Closed) 
                    {
                        _UserSocketsPool.TryRemove(socket);
                    }
                }
                foreach (var socket in _ClusterSocketsPool)
                {
                    if (socket.Value.State == WebSocketState.Closed)
                    {
                        _UserSocketsPool.TryRemove(socket);
                    }
                }
                Thread.Sleep(100);
            }catch
            {
                await ConnectionStateCheck();
            }
        }


        public void AddtoPool(AuthenticationResponse resp,WebSocket session)
        {
            _UserSocketsPool.TryAdd(resp,session);
        }




        public void BroadcastClientEventById(int UserID, EventModel data)
        {
            foreach (var item in _UserSocketsPool)
            {
                if (item.Key.IsUser && item.Key.UserID == UserID.ToString())
                {
                    _WebSocketHandler.SendMessage(item.Value, JsonConvert.SerializeObject(data));
                }
            }
        }


        public void BroadcastClientEvent(EventModel data)
        {
            foreach (var item in _UserSocketsPool)
            {
                if (item.Key.IsUser)
                {
                    _WebSocketHandler.SendMessage(item.Value, JsonConvert.SerializeObject(data));
                }
            }
        }


        public void BroadcastManagerEventByID(int ManagerID, EventModel data)
        {
            foreach (var item in _UserSocketsPool)
            {
                if (item.Key.IsManager && item.Key.UserID == ManagerID.ToString())
                {
                    _WebSocketHandler.SendMessage(item.Value, JsonConvert.SerializeObject(data));
                }
            }
        }

        public void BroadcastAdminEvent(EventModel data)
        {
            foreach(var item in _UserSocketsPool)
            {
                if(item.Key.IsAdmin)
                {
                    _WebSocketHandler.SendMessage(item.Value, JsonConvert.SerializeObject(data));
                }
            }
        }
    }
}
