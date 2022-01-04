using SharedHost.Models.Device;
using System.Collections.Generic;
using SharedHost.Models.User;
using System;
using System.ComponentModel.DataAnnotations;

namespace SharedHost.Models.Cluster
{
    /// <summary>
    /// When the PCC system scale up, slave device will be divided into different Cluster, 
    /// each cluster will have their own slave manager.
    /// </summary>
    public class GlobalCluster
    {
        /// <summary>
        /// Each Cluster have an unique ID
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Name {get;set;}

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string TurnIp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string TurnUser { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string TurnPassword { get;set;}

        /// <summary>
        /// 
        /// </summary>
        public DateTime Register { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime Unregister { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Private { get;set; }

        /// <summary>
        /// Each cluster will contain a certain number of Slave Device
        /// </summary>
        public virtual ICollection<WorkerNode> WorkerNode { get; set; }
    }
}

