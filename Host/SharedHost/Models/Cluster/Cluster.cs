using SharedHost.Models.Device;
using System.Collections.Generic;

namespace SharedHost.Models.Cluster
{
    /// <summary>
    /// When the PCC system scale up, slave device will be divided into different Cluster, 
    /// each cluster will have their own slave manager.
    /// </summary>
    public class Cluster
    {
        /// <summary>
        /// Each Cluster have an unique ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Each cluster will contain a certain number of Slave Device
        /// </summary>
        public virtual ICollection<Slave> Slave { get; set; }
    }
}

