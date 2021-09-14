using Conductor.Models;
using SharedHost.Models.Device;

namespace SharedHost.Models.Session
{
    public class SlaveSession : SessionBase
    {
        public SlaveSession(){}
        public SlaveSession(RemoteSession session,string stun)
        {
            SessionSlaveID = session.SessionSlaveID;

            SignallingUrl = session.SignallingUrl;

            StunServer = stun;

            QoE = session.QoE;

            ClientOffer = session.ClientOffer;

            Slave = session.Slave;
        }

        public Slave Slave { get; set; }
        
        public int SessionSlaveID { get; set; }
    }
}