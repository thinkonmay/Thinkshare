using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SlaveManager.Models
{
    /// <summary>
    /// Slavedevice is the cloud computer device that allow user to access 
    /// </summary>
    public class Slave
    {
        /// <summary>
        /// Each slave device defined with an unique ID 
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// slave device hardware configuration
        /// </summary>
        public string CPU { get; set; }
        public string GPU { get; set; }
        public int RAMcapacity { get; set; }
        public string OS { get; set; }

        /// <summary>
        /// The cluster that this slave device belong to
        /// </summary>
        public virtual Cluster OwnedCluster { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual ICollection<CommandLog> CommandLogs { get; set; }

        /// <summary>
        /// storage which attached to this slave device
        /// </summary>
        public virtual ICollection<StorageBlock>? servingStorage { get; set; }

        /// <summary>
        /// (nullable) if slave is in a session, 
        /// </summary>
        public virtual Session? servingSession { get; set; }
    }
}