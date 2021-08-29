using Conductor.Models;


namespace SharedHost.Models.Session
{
    public class SlaveSession : SessionBase
    {
        public SlaveSession(RemoteSession session,string stun)
        {
            SessionSlaveID = session.SessionSlaveID;

            SignallingUrl = session.SignallingUrl;

            StunServer = stun;

            QoE = session.QoE;

            ClientOffer = session.ClientOffer;

            SlaveID = session.SlaveID;
        }

        public int SlaveID { get; set; }
        public int SessionSlaveID { get; set; }
    }
}