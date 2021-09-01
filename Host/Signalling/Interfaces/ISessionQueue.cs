using SharedHost.Models.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Signalling.Interfaces
{
    public interface ISessionQueue
    {
        public bool AddSessionPair(SessionPair session);
        public bool RemoveIDPair(SessionPair session);
        public WebSocket GetSlaveSocket(int ClientID);
        public WebSocket GetClientSocket(int SlaveID);
        public bool SlaveInQueue(int ClientID);
        public bool ClientInQueue(int SlaveID);
        public bool DeviceIsOnline(int ID);
        public void DevieGoesOffline(int ID);
        public void DeviceGoesOnline(int ID, WebSocket ws);
        public bool SlaveIsOnline(int ClientID);
        public bool ClientIsOnline(int SlaveID);
        public bool IsClient(int ID);
        public bool IsSlave(int ID);

        public List<SessionPair> GetSessionPair();
        public List<int> GetOnlineList();

    }
}
