using System;
using System.Collections.Generic;
using System.Text;
using SharedHost.Models.Device;

namespace SharedHost.Models.Cluster
{
    public class ClusterCredential
    {
        public int ID { get; set; }

        public string ClusterName {get;set;}

        public int OwnerID {get;set;}
    }
}
