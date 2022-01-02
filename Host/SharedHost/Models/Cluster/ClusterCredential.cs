using System;
using System.Collections.Generic;
using System.Text;
using SharedHost.Models.Device;
using System.Linq;

namespace SharedHost.Models.Cluster
{
    public class ClusterCredential
    {
        public string ClusterToken {get;set;}

        public string OwnerToken {get;set;}

        public List<ClusterWorkerNode> WorkerNodes  {get;set;}
    }
}
