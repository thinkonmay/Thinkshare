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

namespace SystemHub.Services
{
    public class WebsocketPool : IWebsocketPool
    {
        private readonly List<KeyValuePair<AuthenticationResponse, WebSocket>> _WebSocketsPool;

        private readonly IWebSocketHandler _WebSocketHandler;

        
        
        public WebsocketPool(SystemConfig config,
                             IWebSocketHandler wsHandler)
        {
            _WebSocketsPool = new List<KeyValuePair<AuthenticationResponse, WebSocket>>();
            _WebSocketHandler = wsHandler;

            Task.Run(() => ConnectionHeartBeat());
        }




        public async Task ConnectionHeartBeat()
        {
            try
            {                
                foreach (var socket in _WebSocketsPool)
                {
                    if(socket.Value.State == WebSocketState.Closed) 
                    {
                        _WebSocketsPool.Remove(socket);
                    }
                }
                Thread.Sleep(100);
            }catch(Exception ex)
            {
                await ConnectionHeartBeat();
            }
        }


        public void AddtoPool(AuthenticationResponse resp,WebSocket session)
        {
            _WebSocketsPool.Add(new KeyValuePair<AuthenticationResponse,WebSocket>(resp,session));
        }




        public void BroadcastClientEventById(int UserID, EventModel data)
        {
            foreach (var item in _WebSocketsPool)
            {
                if (item.Key.IsUser && item.Key.UserID == UserID.ToString())
                {
                    _WebSocketHandler.SendMessage(item.Value, JsonConvert.SerializeObject(data));
                }
            }
        }


        public void BroadcastClientEvent(EventModel data)
        {
            foreach (var item in _WebSocketsPool)
            {
                if (item.Key.IsUser)
                {
                    _WebSocketHandler.SendMessage(item.Value, JsonConvert.SerializeObject(data));
                }
            }
        }


        public void BroadcastManagerEventByID(int ManagerID, EventModel data)
        {
            foreach (var item in _WebSocketsPool)
            {
                if (item.Key.IsManager && item.Key.UserID == ManagerID.ToString())
                {
                    _WebSocketHandler.SendMessage(item.Value, JsonConvert.SerializeObject(data));
                }
            }
        }

        public void BroadcastAdminEvent(EventModel data)
        {
            foreach(var item in _WebSocketsPool)
            {
                if(item.Key.IsAdmin)
                {
                    _WebSocketHandler.SendMessage(item.Value, JsonConvert.SerializeObject(data));
                }
            }
        }
    }
}
