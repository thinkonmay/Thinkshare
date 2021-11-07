using SharedHost.Models.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SystemHub.Interfaces
{
    public interface IWebsocketPool
    {

        public void AddtoClientHub(int ID, WebSocket socket);
        public void AddtoManagerHub(int ID, WebSocket socket);
        public void AddtoAdminHub(WebSocket socket);

        public List<WebSocket> GetClientSockets(int ClientID);
        public List<WebSocket> GetManagerSockets(int ClientID);
        public List<WebSocket> GetAdminSockets();


        public List<WebSocket> GetAllClientSockets();
    }
}
