using System;
using System.Collections.Generic;
using System.Text;

namespace SharedHost.Models
{
    /// <summary>
    /// Contain session initialize request that user send to host
    /// in order to start remote control session
    /// </summary>
    class ClientRequest
    {

        public int ClientId { get; set; }
        /// <summary>
        /// slave id that match with requested user
        /// </summary>
        public int SlaveID { get; set; }

        /// <summary>
        /// quality of experience 
        /// </summary>
        QoE QoE { get; set; }
    }
}
