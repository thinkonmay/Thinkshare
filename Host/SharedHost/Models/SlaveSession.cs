using SlaveManager;
using SlaveManager.Models;

namespace SharedHost.Models
{
    public class SlaveSession : SessionBase
    {
        public SlaveSession(Session session,string stun)
        {
            SessionSlaveID = session.SessionSlaveID;

            SignallingUrl = session.SignallingUrl;

            StunServer = stun;

            QoE = session.QoE;

            ClientOffer = session.ClientOffer;
        }

        public int SessionSlaveID { get; set; }
    }
}