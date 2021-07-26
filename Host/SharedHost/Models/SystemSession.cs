using System;
using System.Collections.Generic;
using System.Text;

namespace SharedHost.Models
{
<<<<<<< Updated upstream
    class SystemSession
=======
    /// <summary>
    /// information of a session that conductor
    /// store inside database,
    /// use for maintainance and management 
    /// </summary>
    public class SystemSession
>>>>>>> Stashed changes
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
