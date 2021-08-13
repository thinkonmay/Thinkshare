using SharedHost.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static System.Environment;

namespace SlaveManager.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Session : SessionBase
    { 
        public Session() { }
        public Session(ClientRequest req,QoE qoe, int sessionSlaveId, int sessionClientId, string signalling, string stun)
        {
            ClientID = req.ClientId;
            SlaveID = req.SlaveId;

            SessionSlaveID = sessionSlaveId;
            SessionClientID = sessionClientId;

            QoE = qoe;
            ClientOffer = false;
            SignallingUrl = signalling;
            StunServer = stun;
        }


        /// <summary>
        /// 
        /// </summary>
        public int ClientID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int SlaveID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Key, Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SessionSlaveID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Key, Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SessionClientID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}