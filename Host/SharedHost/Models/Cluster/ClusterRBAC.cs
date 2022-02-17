using SharedHost.Models.Device;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using SharedHost.Models.AWS;
using SharedHost.Models.User;

namespace SharedHost.Models.Cluster
{
    public class ClusterRole
    {
        [Key]
        public int ID { get; set; }

        public DateTime? Start { get; set; }

        public DateTime? Endtime { get; set; }

        public string Description {get;set;}

        public int ClusterID {get;set;}

        [JsonIgnore]
        [ForeignKey("ClusterID")]
        public virtual GlobalCluster? Cluster {get;set;}

        [Required]
        public int UserID {get;set;}

        [JsonIgnore]
        [ForeignKey("UserID")]
        public virtual UserAccount User { get; set; }
    }

    public class ClusterRoleRequest
    {
        public DateTime? Start { get; set; }

        public DateTime? Endtime { get; set; }

        public string Description {get;set;}

        public string User {get;set;}
    }
}

