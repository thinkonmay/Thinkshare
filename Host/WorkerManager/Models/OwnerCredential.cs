using SharedHost.Models.Cluster;
using System;

namespace WorkerManager.Models
{
    public class OwnerCredential
    {
        public string Name {  get; set; }
        public string? Description { get; set; }
        public string token { get;set; }
        public LocalCluster WorkerCluster { get; set; }
        public DateTime ValidUntil { get; set; }
    }
}
