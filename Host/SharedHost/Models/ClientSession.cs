using SlaveManager;
using SlaveManager.Models;

namespace SharedHost.Models
{
    public class ClientSession : SessionBase
    {
        public ClientSession(Session session, string stun)
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