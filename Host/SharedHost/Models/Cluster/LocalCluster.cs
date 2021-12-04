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


        [Required]
        public string Name {get;set;}

        /// <summary>
        /// 
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime Register { get; set; }
    }
}

