
using Conductor.Models;
using SharedHost;


namespace SharedHost.Models.Session
{
    public class ClientSession : SessionBase
    {
        public ClientSession(RemoteSession session, SystemConfig config)
        {

            SessionClientID = session.SessionClientID;

            SignallingUrl = session.SignallingUrl;

            QoE = session.QoE;
            
            turn = config.TurnServer;

            turn.urls = "turn:" + config.TurnServer.urls;
        }

        public TurnServer turn {get;set;}

        public int SessionClientID { get; set; }
    }
}