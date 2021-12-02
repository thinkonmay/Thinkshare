using SharedHost.Models.Device;
using System.Collections.Generic;
using SharedHost.Models.User;
using System;
using System.ComponentModel.DataAnnotations;

namespace SharedHost.Models.Cluster
{
    public class LocalCluster
    {
        /// <summary>
        /// Each Cluster have an unique ID
        /// </summary>
        [Key]
        public int ID { get; set; }


        public string Name {get;set;}

        /// <summary>
        /// 
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? TurnIP { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? TurnUser { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? TurnPassword { get;set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime Register { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Private { get;set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Enabled {  get; set; }

        /// <summary>
        /// Each cluster will contain a certain number of Slave Device
        /// </summary>
        public virtual ICollection<WorkerNode> Slave { get; set; }
    }
}

