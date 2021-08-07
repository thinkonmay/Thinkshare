using SlaveManager;
using SlaveManager.Models;

namespace SharedHost.Models
{
    public class ClientSession : SessionBase
    {
        public ClientSession(Session session)
        {

            SessionClientID = session.SessionClientID;

            SignallingUrl = session.SignallingUrl;

            StunServer = GeneralConstants.STUN_SERVER_GSTREAMER_FORMAT;

            QoE = session.QoE;

            ClientOffer = session.ClientOffer;
        }
        public int SessionClientID { get; set; }
    }
}