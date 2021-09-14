using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SharedHost.Models.Device;
using SharedHost.Models.User;

namespace SharedHost.Models.Session
{
    /// <summary>
    /// 
    /// </summary>
    public class RemoteSession : SessionBase
    { 
        public RemoteSession() { }

        public RemoteSession(QoE qoe, 
                             SessionPair pair, 
                             SystemConfig config)
        {
            SessionSlaveID = pair.SessionSlaveID;
            SessionClientID = pair.SessionClientID;

            QoE = qoe;
            ClientOffer = false;
            SignallingUrl = config.SignallingWs; ;
            StunServer = config.StunServer;
            // StartTime = DateTime.Now;
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual UserAccount Client { get; set; }


        public virtual Slave Slave { get; set; }

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