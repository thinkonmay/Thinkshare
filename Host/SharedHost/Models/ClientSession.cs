using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;


namespace SharedHost.Models
{
    /// <summary>
    /// SessionClient Package send to client,
    /// provide sufficient information for client to 
    /// start a remote control session with slave
    /// </summary>
    public class ClientSession
    {
        /// <summary>
        /// Session Client ID send to client,
        /// use to register with signalling server
        /// </summary>
        public int SessionClientID { get; set; }

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