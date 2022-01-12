using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SharedHost.Models.AWS
{
    public class ClusterInstance : EC2Instance
    {
        [Key]
        public int ID {get;set;}

        public string TurnUser {get;set;}

        public string TurnPassword {get;set;}

        public DateTime? Registered {get;set;}

        public virtual List<PortForward> portForwards {get;set;}
    }
}
