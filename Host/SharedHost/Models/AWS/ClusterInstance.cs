using System;
using System.Collections.Generic;
using System.Text;

namespace SharedHost.Models.AWS
{
    public class ClusterInstance
    {
        public EC2Instance instance { get; set; }

        public string TurnUser {get;set;}

        public string TurnPassword {get;set;}
    }
}
