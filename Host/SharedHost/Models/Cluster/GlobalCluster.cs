using SharedHost.Models.Device;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SharedHost.Models.AWS;
using SharedHost.Models.User;

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

        public DateTime? Register { get; set; }

        public DateTime? Unregister { get; set; }

        public bool SelfHost {get;set;}

        public virtual List<WorkerNode> WorkerNode { get; set; }

        public int? InstanceID {get;set;}

        [ForeignKey("InstanceID")]
        public virtual ClusterInstance? instance {get;set;}

        [Required]
        public int OwnerID {get;set;}

        [ForeignKey("OwnerID")]
        public virtual UserAccount Owner { get; set; }
    }
}

