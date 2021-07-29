using SharedHost.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace SlaveManager.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Session : SystemSession
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int ClusterID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual Cluster cluster { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? EndTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [NotMapped]
        public virtual new QoE QoE { get; set; }


    }
}