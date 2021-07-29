namespace SharedHost.Models
{
    /// <summary>
    /// information of a session that conductor
    /// store inside database,
    /// use for maintainance and management 
    /// </summary>
    public class SystemSession
    {
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
        public int SessionSlaveID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int SessionClientID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SignallingUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string StunServer { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public bool ClientOffer { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public QoE QoE { get; set; }
    }
}
