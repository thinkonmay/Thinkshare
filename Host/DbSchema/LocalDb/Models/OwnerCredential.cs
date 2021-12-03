using SharedHost.Models.Cluster;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DbSchema.LocalDb.Models
{
    public class OwnerCredential
    {
        [Key]
        public string Name {  get; set; }
        public string? Description { get; set; }
        public string token { get;set; }
        public virtual LocalCluster WorkerCluster { get; set; }
        public virtual DateTime ValidUntil { get; set; }
    }
}
