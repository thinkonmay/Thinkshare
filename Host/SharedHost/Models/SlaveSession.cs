using SlaveManager;
using SlaveManager.Models;

namespace SharedHost.Models
{
    public class SlaveSession : SessionBase
    {
        public SlaveSession(Session session)
        {
            SessionSlaveID = session.SessionSlaveID;

            SignallingUrl = session.SignallingUrl;

            StunServer = GeneralConstants.STUN_SERVER_GSTREAMER_FORMAT;

            QoE = session.QoE;

            ClientOffer = session.ClientOffer;
        }

        public int SessionSlaveID { get; set; }
    }
}