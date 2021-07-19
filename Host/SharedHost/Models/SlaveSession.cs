using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;


namespace SharedHost.Models
{
    /// <summary>
    /// SlaveSession Package send to agent,
    /// provide sufficient information for agent to 
    /// start a remote control session with client
    /// </summary>
    public class SlaveSession
    {
        /// <summary>
        /// Session Slave ID send to slave,
        /// use to register with signalling server
        /// </summary>
        public int SessionSlaveID { get; set; }

        /// <summary>
        /// Signalling server url
        /// </summary>
        public string SignallingUrl { get; set; }
        
        /// <summary>
        /// Stun-server url
        /// </summary>
        public string StunServer { get; set; }

        /// <summary>
        /// Client offer first or not,
        /// dont affect much
        /// default false
        /// </summary>
        public bool ClientOffer { get; set; }

        /// <summary>
        /// Quality of experience of session
        /// if clientrequest inside capability of slave device
        /// Host dont modify this object
        /// </summary>
        public QoE QoE { get; set; }
    }
}