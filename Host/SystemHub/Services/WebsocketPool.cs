using RestSharp;
using SharedHost;
using SharedHost.Models.Session;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using SystemHub.Interfaces;

namespace SystemHub.Services
{
    public class WebsocketPool : IWebsocketPool
    {
        public WebsocketPool(SystemConfig config)
        {
            clientHub  = new ConcurrentDictionary<int, List<WebSocket>>();
            managerHub   = new ConcurrentDictionary<int, List<WebSocket>>();
            adminHub = new List<WebSocket>();

            Task.Run(() => ConnectionHeartBeat());
        }

        public async Task ConnectionHeartBeat()
        {
            try
            {
                foreach (var groupsocket in clientHub.Values)
                {
                    if(groupsocket.Count() == 0)
                    {
                        clientHub.TryRemove(clientHub.Where(o => o.Value == groupsocket));
                    }
                    foreach (var socket in groupsocket)
                    {
                        if(socket.State == WebSocketState.Closed) 
                        {
                            groupsocket.Remove(socket);
                        }
                    }   
                }

                foreach (var groupsocket in managerHub.Values)
                {
                    if(groupsocket.Count() == 0)
                    {
                        clientHub.TryRemove(clientHub.Where(o => o.Value == groupsocket));
                    }
                    foreach (var socket in groupsocket)
                    {
                        if(socket.State == WebSocketState.Closed) 
                        {
                            groupsocket.Remove(socket);
                        }
                    }   
                }
                
                foreach (var socket in adminHub)
                {
                    if(socket.State == WebSocketState.Closed) 
                    {
                        groupsocket.Remove(socket);
                    }
                }
                Thread.Sleep(100);
            }catch(Exception ex)
            {
                await ConnectionHeartBeat();
            }
        }
        private ConcurrentDictionary<int, List<WebSocket>> clientHub;
        private ConcurrentDictionary<int, List<WebSocket>> managerHub;
        private List<WebSocket> adminHub;


        public void AddtoAdminHub(WebSocket session)
        {
            adminHub.Add(session);
        }

        public void AddtoClientHub(int ID, WebSocket socket)
        {
            if(clientHub.Where(o => o.Key == ID).Count() > 0)
            {
                clientHub.Where(o => o.Key == ID).FirstOrDefault().Value.Add(socket);
            }
            else
            {
                var newClientKey = new KeyValuePair<int, List<WebSocket>>(ID, new List<WebSocket>{socket});
                clientHub.TryAdd(newClientKey);
            }
        }

        public void AddtoManagerHub(int ID, WebSocket socket)
        {
            if(managerHub.Where(o => o.Key == ID).Count() > 0)
            {
                managerHub.Where(o => o.Key == ID).FirstOrDefault().Value.Add(socket);
            }
            else
            {
                var newClientKey = new KeyValuePair<int, List<WebSocket>>(ID, new List<WebSocket>{socket});
                managerHub.TryAdd(newClientKey);
            }
        }





















        public List<WebSocket> GetClientSockets(int ClientID)
        {
            var ret = new List<WebSocket>();
            foreach (var i in clientHub)
            {
                foreach (var j in i.Value)
                {
                    if (j.State == WebSocketState.Closed)
                    {
                        i.Value.Remove(j);
                    }
                    else if (j.State == WebSocketState.Open)
                    {
                        if (i.Key == ClientID)
                        {
                            ret.Add(j);
                        }
                    }
                }
            }
            return ret;
        }


        public List<WebSocket> GetManagerSockets(int ManagerID)
        {
            var ret = new List<WebSocket>();
            foreach (var i in managerHub)
            {
                foreach (var j in i.Value)
                {
                    if (j.State == WebSocketState.Closed)
                    {
                        i.Value.Remove(j);
                    }
                    else if (j.State == WebSocketState.Open)
                    {
                        if(i.Key == ManagerID)
                        {
                            ret.Add(j);
                        }
                    }
                }
            }
            return ret;
        }

        public List<WebSocket> GetAdminSockets()
        {
            return adminHub;
        }

        public List<WebSocket> GetAllClientSockets()
        {
            var ret = new List<WebSocket>();
            foreach (var i in clientHub)
            {
                foreach (var j in i.Value)
                {
                    if (j.State == WebSocketState.Closed)
                    {
                        i.Value.Remove(j);
                    }
                    else if (j.State == WebSocketState.Open)
                    {
                        ret.Add(j);                        
                    }
                }
            }
            return ret;
        }
    }
}
