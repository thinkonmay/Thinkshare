using System;
using System.Collections.Generic;
using System.Text;

namespace SharedHost.Models
{
    public class SystemSession
    {
        public int ClientID { get; set; }

        public int SlaveID { get; set; }

        public int SessionSlaveID { get; set; }
        
        public int SessionClientID { get; set; }

        public string SignallingUrl { get; set; }

        public string StunServer { get; set; }

        public bool ClientOffer { get; set; }

        public QoE QoE { get; set; }
    }
}
