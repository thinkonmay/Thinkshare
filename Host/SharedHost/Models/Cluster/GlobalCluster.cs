using SharedHost.Models.Device;
using System.Collections.Generic;
using SharedHost.Models.User;
using System;
using System.ComponentModel.DataAnnotations;
using SharedHost.Models.AWS;

namespace SharedHost.Models.Cluster
{
    /// <summary>
    /// When the PCC system scale up, slave device will be divided into different Cluster, 
    /// each cluster will have their own slave manager.
    /// </summary>
    public class GlobalCluster
    {
        [Key]
        public int ID { get; set; }

        public string Name {get;set;}

        public DateTime Register { get; set; }

        public DateTime Unregister { get; set; }

        public bool Private { get;set; }

        public bool SelfHost {get;set;}

        public virtual ICollection<WorkerNode> WorkerNode { get; set; }

        public virtual ClusterInstance? instance {get;set;}
    }
}

