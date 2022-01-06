using System.Collections.Generic;

namespace WorkerManager.Models
{
    public class ClusterKey
    {
        public string Name {get;set;}

        public string OwnerName {get;set;}

        public string ClusterToken {get;set;}

        public string OwnerToken {get;set;}

        public List<ClusterWorkerNode> WorkerNodes  {get;set;}
    }
}
