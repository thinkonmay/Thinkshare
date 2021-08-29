
using Conductor.Models;


namespace SharedHost.Models.Session
{
    public class ClientSession : SessionBase
    {
        public ClientSession(RemoteSession session, string stun)
        {

            SessionClientID = session.SessionClientID;

            SignallingUrl = session.SignallingUrl;

            StunServer = stun;

            QoE = session.QoE;

            ClientOffer = session.ClientOffer;
        }
        public int SessionClientID { get; set; }
    }
}