using RestSharp;
using SharedHost;
using SharedHost.Models.Session;
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
        public SessionQueue(SystemConfig config)
        {
            onlineList = new ConcurrentDictionary<int, WebSocket>();

            sessionPairs = new List<SessionPair>();

            _conductor = new RestClient(config.Conductor + "/ReportSession");
        }

        private readonly RestClient _conductor;

        private ConcurrentDictionary<int, WebSocket> onlineList;

        private List<SessionPair> sessionPairs; //<ClientID, SlaveID>


        public bool AddSessionPair(SessionPair session)
        {
            if(sessionPairs.Where(o => o == session).Count() > 0) { return false; }
            sessionPairs.Add(session);
            return true;
        }

        public bool RemoveIDPair(SessionPair session)
        {
            WebSocket ws1, ws2;
            var ret = sessionPairs.Remove(session);
            onlineList.TryRemove(session.SessionSlaveID, out ws2);
            onlineList.TryRemove(session.SessionClientID, out ws1);
            try
            {
                if (ws1 != null) { ws1.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None); }
                if (ws2 != null) { ws2.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None); }
            }catch (WebSocketException){  }

            if(ret)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public WebSocket GetSlaveSocket(int ClientID)
        {
            var session = sessionPairs.Where(o => o.SessionClientID == ClientID).FirstOrDefault();
            if (onlineList.Where(i => i.Key == session.SessionSlaveID).Count() == 1)
            {
                return onlineList.Where(i => i.Key == session.SessionSlaveID).FirstOrDefault().Value;
            }
            else
            {
                return null;
            }
        }

        public WebSocket GetClientSocket(int SlaveID)
        {
            var session = sessionPairs.Where(o => o.SessionSlaveID == SlaveID).FirstOrDefault();
            if (onlineList.Where(i => i.Key == session.SessionClientID).Count() == 1)
            {
                return onlineList.Where(i => i.Key == session.SessionClientID).FirstOrDefault().Value;
            }
            else
            {
                return null;
            }
        }

        public bool ClientInQueue(int ClientID)
        {
            if (sessionPairs.Where(o => o.SessionClientID == ClientID).Count() > 0)
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
            if (sessionPairs.Where(o => o.SessionSlaveID == SlaveID).Count() > 0)
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
            var SlaveID = sessionPairs
                .Where(o => o.SessionSlaveID == ClientID).FirstOrDefault()
                .SessionSlaveID;

            return DeviceIsOnline(SlaveID);
        }

        public bool ClientIsOnline(int SlaveID)
        {
            var ClientID = sessionPairs
                .Where(o => o.SessionSlaveID == SlaveID).FirstOrDefault()
                .SessionClientID;

            return DeviceIsOnline(ClientID);
        }



        public void DevieGoesOffline(int ID)
        {
            WebSocket mock;
            onlineList.TryRemove(ID, out mock);
            var session = sessionPairs
                .Where(o => o.SessionClientID == ID || o.SessionSlaveID == ID).FirstOrDefault();

            if(session == null) { return;  }
            var request = new RestRequest("SignallingDisconnected")
                .AddJsonBody(session);

            _conductor.Post(request);
        }

        public void DeviceGoesOnline(int ID, WebSocket ws)
        {
            WebSocket value;
            onlineList.TryRemove(ID, out value);
            onlineList.TryAdd(ID, ws);
        }

        public bool IsClient(int ID)
        {
            return (sessionPairs.Where(session => session.SessionClientID == ID).Count() > 0) ? true : false;
        }

        public bool IsSlave(int ID)
        {
            return (sessionPairs.Where(session => session.SessionSlaveID == ID).Count() > 0) ? true : false;
        }


        public List<SessionPair> GetSessionPair()
        {
            return sessionPairs;
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
