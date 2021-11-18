using SharedHost.Models.Device;
using System.Collections.Generic;
using SharedHost.Models.User;
using System;

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
        /// 
        /// </summary>
        public DateTime Register { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Private { get;set; }

        /// <summary>
        /// Each cluster will contain a certain number of Slave Device
        /// </summary>
        public virtual ICollection<WorkerNode> Slave { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual UserAccount ManagerAccount {get;set;}
    }
}

