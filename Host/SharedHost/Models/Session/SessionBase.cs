using System.ComponentModel.DataAnnotations.Schema;

namespace SharedHost.Models.Session
{
    /// <summary>
    /// information of a session that conductor
    /// store inside database,
    /// use for maintainance and management 
    /// </summary>
    public class SessionBase
    {
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
        public virtual QoE QoE { get; set; }
    }
}
