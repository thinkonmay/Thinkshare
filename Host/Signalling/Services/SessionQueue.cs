using Signalling.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Signalling.Services
{
    public class SessionQueue : ISessionQueue
    {
        public SessionQueue()
        {
            onlineList = new ConcurrentDictionary<int, WebSocket>();

            sessionPair = new ConcurrentDictionary<int, int>();

            sessionPair.TryAdd(456, 123);
        }

        private ConcurrentDictionary<int, WebSocket> onlineList;

        private ConcurrentDictionary<int, int> sessionPair; //<ClientID, SlaveID>


        public void AddSessionPair(int slaveID, int clientID)
        {
            sessionPair.TryAdd(clientID, slaveID);
            return;
        }

        public void RemoveIDPair(int SlaveID, int ClientID)
        {
            WebSocket ws1, ws2;
            onlineList.TryRemove(SlaveID, out ws2);
            onlineList.TryRemove(ClientID, out ws1);

            if (ws1 != null) { ws1.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None); }
            if (ws2 != null) { ws2.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None); }

            sessionPair.TryRemove(ClientID, out SlaveID);
        }

        public WebSocket GetSlaveSocket(int ClientID)
        {
            var SlaveID = sessionPair.Where(o => o.Key == ClientID).FirstOrDefault().Value;
            if (onlineList.Where(i => i.Key == SlaveID).Count() == 1)
            {
                return onlineList.Where(i => i.Key == SlaveID).FirstOrDefault().Value;
            }
            else
            {
                return null;
            }     
        }

        public WebSocket GetClientSocket(int SlaveID)
        {
            var ClientID = sessionPair.Where(o => o.Value == SlaveID).FirstOrDefault().Key;
            if (onlineList.Where(i => i.Key == ClientID).Count() == 1)
            {
                return onlineList.Where(i => i.Key == ClientID).FirstOrDefault().Value;
            }
            else
            {
                return null;
            }
        }

        public bool ClientInQueue(int ClientID)
        {
            if (sessionPair.Where(o => o.Key == ClientID).Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public bool SlaveInQueue(int SlaveID)
        {
            if (sessionPair.Where(o => o.Value == SlaveID).Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool DeviceIsOnline(int ID)
        {
            if (onlineList.Where(o => o.Key == ID).Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SlaveIsOnline(int ClientID)
        {
            var SlaveID = sessionPair.Where(o => o.Key == ClientID).FirstOrDefault().Value;

            return DeviceIsOnline(SlaveID);
        }

        public bool ClientIsOnline(int SlaveID)
        {
            var ClientID  = sessionPair.Where(o => o.Value == SlaveID).FirstOrDefault().Key;

            return DeviceIsOnline(ClientID);
        }



        public void DevieGoesOffline(int ID)
        {
            WebSocket mock;
            onlineList.TryRemove(ID, out mock);
        }

        public void DeviceGoesOnline(int ID, WebSocket ws)
        {
            WebSocket value;
            onlineList.TryRemove(ID, out value);
            onlineList.TryAdd(ID, ws);
        }

        public bool IsClient(int ID)
        {
            return sessionPair.ContainsKey(ID);
        }

        public bool IsSlave(int ID)
        {
            return (sessionPair.Where(o => o.Value == ID).Count() > 0) ? true : false;
        }


        public List<Tuple<int, int>> GetSessionPair()
        {
            List<Tuple<int, int>> ret = new List<Tuple<int, int>>();
            foreach (var i in sessionPair)
            {
                ret.Add(new Tuple<int, int>(i.Key, i.Value));
            }
            return ret;
        }

        public List<int> GetOnlineList()
        {
            List<int> ret = new List<int>();
            foreach (var i in onlineList)
            {
                ret.Add(i.Key);
            }
            return ret;
        }
    }
}
