using Conductor.Models;
using SharedHost.Models.Device;
using SharedHost;

namespace SharedHost.Models.Session
{
    public class SlaveSession : SessionBase
    {
        public SlaveSession(){}
        public SlaveSession(RemoteSession session,SystemConfig config)
        {
            SessionSlaveID = session.SessionSlaveID;

            SignallingUrl = session.SignallingUrl;
            
            QoE = session.QoE;

            SlaveID = session.Slave.ID;

            TurnConnection =  "turn://" + config.TurnServer.username + ":" + config.TurnServer.credentials + "@" + urls;
        }

        public int SlaveID { get; set; }
        
        public int SessionSlaveID { get; set; }

        public string TurnConnection {get;set;}
    }
}