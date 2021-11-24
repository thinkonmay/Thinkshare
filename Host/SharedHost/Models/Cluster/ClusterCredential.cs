using System;
using System.Collections.Generic;
using System.Text;
using SharedHost.Models.Device;

namespace SharedHost.Models.Cluster
{
    public class ClusterCredential
    {
        public int ID { get; set; }

        public ICollection<WorkerNode> Devices { get;set; }
    }
}
