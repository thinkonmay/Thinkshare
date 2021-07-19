using System;
using System.Collections.Generic;
using System.Text;

namespace SharedHost.Models
{
    /// <summary>
    /// information of a session that conductor
    /// store inside database,
    /// use for maintainance and management 
    /// </summary>
    class SystemSession
    {
        /// <summary>
        /// ID of client
        /// </summary>
        public int ClientID { get; set; }

        /// <summary>
        /// ID of Slave
        /// </summary>
        public int SlaveID { get; set; }

        /// <summary>
        /// Session slave id send to slave to register session
        /// with signalling server
        /// </summary>
        public int SessionSlaveID { get; set; }

        /// <summary>
        /// Session client id send to client to register session
        /// with signalling server
        /// </summary>
        public int SessionClientID { get; set; }

        /// <summary>
        /// signalling server url use to register session
        /// </summary>
        public string SignallingUrl { get; set; }

        /// <summary>
        /// stun server used between client and slave
        /// </summary>
        public string StunServer { get; set; }

        /// <summary>
        /// who offer sdp first
        /// </summary>
        public bool ClientOffer { get; set; }

        /// <summary>
        /// quality of experience of the session
        /// </summary>
        public QoE QoE { get; set; }
    }
}
